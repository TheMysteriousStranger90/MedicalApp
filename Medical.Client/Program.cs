using Medical.Client.Extensions;
using Medical.Client.Interceptors;
using Medical.Client.Interfaces;
using Medical.Client.Middleware;
using Medical.Client.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .Enrich.WithProperty("Application", "Medical.Client")
    .Enrich.FromLogContext()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = "MedicalAuth";
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequirePatientRole", policy => policy.RequireRole("Patient"));
    options.AddPolicy("RequireDoctorRole", policy => policy.RequireRole("Doctor"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
    });
});

builder.Services.AddSingleton<ITokenStorageService, LocalStorageTokenService>();
builder.Services.AddScoped<AuthInterceptor>();

builder.Services.AddGrpcClients(builder.Configuration);

builder.Services.AddHealthChecks();

builder.Services.AddScoped<IAuthenticationService, AuthenticationServiceGrpc>();
builder.Services.AddScoped<IAppointmentService, AppointmentServiceGrpc>();
builder.Services.AddScoped<IDoctorService, DoctorServiceGrpc>();
builder.Services.AddScoped<IPatientService, PatientServiceGrpc>();
builder.Services.AddScoped<IMedicalRecordService, MedicalRecordServiceGrpc>();

try
{
    Log.Information("Starting Medical Client web application");
    var app = builder.Build();
    
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();
    app.UseCors("AllowAll");
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseMiddleware<AuthenticationMiddleware>();
    
    app.MapHealthChecks("/health");

    app.MapRazorPages();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}