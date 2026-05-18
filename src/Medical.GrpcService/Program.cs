using Medical.GrpcService.Context;
using Medical.GrpcService.Extensions;
using Medical.GrpcService.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Medical.GrpcService");

    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, cfg) =>
        cfg.ReadFrom.Configuration(ctx.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());

    builder.WebHost.ConfigureKestrel((ctx, options) =>
    {
        options.Limits.MaxConcurrentConnections = 1000;
        options.Limits.MaxConcurrentUpgradedConnections = 1000;
        options.Limits.Http2.MaxStreamsPerConnection = 100;

        if (ctx.HostingEnvironment.IsDevelopment())
        {
            // Use ListenAnyIP so the service is reachable from other Docker containers
            // via the bridge network (ListenLocalhost only binds 127.0.0.1 and is
            // invisible to sibling containers).
            var devCertPath     = ctx.Configuration["Kestrel:Certificates:Default:Path"];
            var devCertPassword = ctx.Configuration["Kestrel:Certificates:Default:Password"];

            options.ListenAnyIP(7084, o =>
            {
                o.Protocols = HttpProtocols.Http2;
                // When running inside Docker a PFX is mounted and its path injected via
                // ASPNETCORE_Kestrel__Certificates__Default__Path; use it.
                // For plain "dotnet run" outside Docker fall back to the ASP.NET dev-cert.
                if (!string.IsNullOrEmpty(devCertPath) && File.Exists(devCertPath))
                    o.UseHttps(devCertPath, devCertPassword);
                else
                    o.UseHttps(); // ASP.NET Core development certificate (local dev)
            });
            options.ListenAnyIP(5006, o => { o.Protocols = HttpProtocols.Http1; });
        }
        else
        {
            string certPath = ctx.Configuration["Kestrel:Certificates:Default:Path"]
                              ?? throw new InvalidOperationException(
                                  "Kestrel:Certificates:Default:Path must be configured in Production. " +
                                  "Set via Kestrel__Certificates__Default__Path environment variable.");

            string? certPassword = ctx.Configuration["Kestrel:Certificates:Default:Password"];
            if (string.IsNullOrEmpty(certPassword))
            {
                throw new InvalidOperationException(
                    "Kestrel:Certificates:Default:Password must be set in Production. " +
                    "Set via Kestrel__Certificates__Default__Password environment variable.");
            }

            // HTTP/2 + TLS for gRPC
            options.ListenAnyIP(7084, o =>
            {
                o.Protocols = HttpProtocols.Http2;
                o.UseHttps(certPath, certPassword);
            });

            // Plain HTTP/1.1 for Kubernetes liveness/readiness probes and internal load balancers
            options.ListenAnyIP(8080, o => { o.Protocols = HttpProtocols.Http1; });
        }
    });

    builder.Services.Configure<HostOptions>(o =>
        o.ShutdownTimeout = TimeSpan.FromSeconds(30));

    builder.Services.AddApplicationServices(builder.Configuration);
    builder.Services.AddIdentityServices(builder.Configuration);

    builder.Services.AddGrpc(options =>
    {
        options.EnableDetailedErrors = builder.Environment.IsDevelopment();
        options.MaxReceiveMessageSize = 16 * 1024 * 1024; // 16 MB
        options.MaxSendMessageSize = 16 * 1024 * 1024; // 16 MB
    });

    if (builder.Environment.IsDevelopment())
        builder.Services.AddGrpcReflection();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowGrpcWeb", policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .WithExposedHeaders(
                    "Grpc-Status", "Grpc-Message",
                    "Grpc-Encoding", "Grpc-Accept-Encoding");
        });
    });

    builder.Services.AddOpenTelemetryObservability(builder.Configuration);

    WebApplication app = builder.Build();

    try
    {
        using IServiceScope scope = app.Services.CreateScope();
        ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.MigrateAndCreateDataAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Database migration or seeding failed — cannot start service");
        throw;
    }

    if (app.Environment.IsDevelopment())
        app.UseDeveloperExceptionPage();
    else
    {
        app.UseExceptionHandler(exBuilder =>
            exBuilder.Run(ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
                return Task.CompletedTask;
            }));
    }

    app.UseRouting();
    app.UseCors("AllowGrpcWeb");
    app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapPrometheusScrapingEndpoint();

    app.MapGrpcService<AuthenticationGrpcService>().EnableGrpcWeb();
    app.MapGrpcService<AppointmentGrpcService>().EnableGrpcWeb();
    app.MapGrpcService<DoctorGrpcService>().EnableGrpcWeb();
    app.MapGrpcService<PatientGrpcService>().EnableGrpcWeb();
    app.MapGrpcService<MedicalRecordGrpcService>().EnableGrpcWeb();

    if (app.Environment.IsDevelopment())
        app.MapGrpcReflectionService();

    // Liveness: is the process up? (no heavy checks)
    app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });

    // Readiness: are all dependencies (DB etc.) ready to serve traffic?
    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = hc => hc.Tags.Contains("ready")
    });

    // Aggregate endpoint for backwards compatibility
    app.MapHealthChecks("/health");

    app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");
    app.MapGet("/version", () => Results.Ok(new
    {
        Version = "1.1.0",
        Environment = app.Environment.EnvironmentName,
        MachineName = Environment.MachineName
    }));
    app.MapGet("/docs", () => Results.Redirect("https://github.com/TheMysteriousStranger90/MedicalApp"));

    await app.RunAsync();
}
catch (Exception ex) when (ex is not OperationCanceledException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

return 0;
