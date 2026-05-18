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

    var builder = WebApplication.CreateBuilder(args);

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
            options.ListenLocalhost(7084, o =>
            {
                o.Protocols = HttpProtocols.Http2;
                o.UseHttps(); // ASP.NET Core development certificate
            });
            options.ListenLocalhost(5006, o => { o.Protocols = HttpProtocols.Http1; });
        }
        else
        {
            var certPath = ctx.Configuration["Kestrel:Certificates:Default:Path"]
                ?? throw new InvalidOperationException(
                    "Kestrel:Certificates:Default:Path must be configured in Production. " +
                    "Set via Kestrel__Certificates__Default__Path environment variable.");

            var certPassword = ctx.Configuration["Kestrel:Certificates:Default:Password"];
            if (string.IsNullOrEmpty(certPassword))
                throw new InvalidOperationException(
                    "Kestrel:Certificates:Default:Password must be set in Production. " +
                    "Set via Kestrel__Certificates__Default__Password environment variable.");

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
        options.MaxSendMessageSize   = 16 * 1024 * 1024; // 16 MB
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

    var app = builder.Build();

    try
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.MigrateAndCreateDataAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Database migration or seeding failed — cannot start service");
        throw;
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
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
        MachineName = System.Environment.MachineName
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
