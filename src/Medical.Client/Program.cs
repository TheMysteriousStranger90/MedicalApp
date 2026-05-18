using Medical.Client.Extensions;
using Medical.Client.Interceptors;
using Medical.Client.Interfaces;
using Medical.Client.Middleware;
using Medical.Client.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Medical.Client");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, cfg) =>
        cfg.ReadFrom.Configuration(ctx.Configuration)
           .ReadFrom.Services(services)
           .Enrich.WithProperty("Application", "Medical.Client")
           .Enrich.FromLogContext());

    builder.Services.Configure<HostOptions>(o =>
        o.ShutdownTimeout = TimeSpan.FromSeconds(30));

    builder.Services.AddRazorPages();
    builder.Services.AddHttpContextAccessor();

    builder.Services.AddAntiforgery(o =>
    {
        o.Cookie.Name       = "_MedicalXsrf";
        o.Cookie.HttpOnly   = true;
        o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        o.Cookie.SameSite   = SameSiteMode.Strict;
        o.HeaderName        = "X-Csrf-Token";
    });

    builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme          = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            options.LoginPath       = "/Account/Login";
            options.LogoutPath      = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.Cookie.Name       = "MedicalAuth";
            options.Cookie.HttpOnly   = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.Cookie.SameSite   = SameSiteMode.Strict;
            options.Cookie.IsEssential = true;
            options.ExpireTimeSpan    = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
        });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("RequirePatientRole", p => p.RequireRole("Patient"));
        options.AddPolicy("RequireDoctorRole",  p => p.RequireRole("Doctor"));
    });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
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

    builder.Services.AddHealthChecks();

    builder.Services.AddSingleton<ITokenStorageService, LocalStorageTokenService>();
    builder.Services.AddScoped<GrpcClientInterceptor>();

    // Pass environment so dev can use self-signed certs; prod uses proper validation
    builder.Services.AddGrpcClients(builder.Configuration, builder.Environment);

    builder.Services.AddScoped<IAuthenticationService, AuthenticationServiceGrpc>();
    builder.Services.AddScoped<IAppointmentService, AppointmentServiceGrpc>();
    builder.Services.AddScoped<IDoctorService, DoctorServiceGrpc>();
    builder.Services.AddScoped<IPatientService, PatientServiceGrpc>();
    builder.Services.AddScoped<IMedicalRecordService, MedicalRecordServiceGrpc>();

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    // Security headers for all responses
    app.Use(async (ctx, next) =>
    {
        ctx.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        ctx.Response.Headers.Append("X-Frame-Options", "DENY");
        ctx.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        await next();
    });

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseCors("AllowAll");
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseMiddleware<AuthenticationMiddleware>();

    // Liveness: is the process alive? (no dependency checks)
    app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });

    // Readiness: all dependencies ready?
    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = hc => hc.Tags.Contains("ready")
    });

    app.MapHealthChecks("/health");
    app.MapRazorPages();

    await app.RunAsync();
    return 0;
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
