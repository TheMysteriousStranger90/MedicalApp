using Medical.Client;
using Medical.Client.Configuration;
using Medical.Client.Interceptors;
using Medical.Client.Interfaces;
using Medical.Client.Middleware;
using Medical.Client.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Core services
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

// Authentication setup
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
    options.AddPolicy("AllowGrpcWeb", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding",
                "Grpc-Accept-Encoding");
    });
});

// Authentication and storage
builder.Services.AddSingleton<ITokenStorageService, LocalStorageTokenService>();
builder.Services.AddScoped<AuthInterceptor>();

// gRPC client registration
var grpcConfig = builder.Configuration.GetSection("GrpcClient").Get<GrpcClientConfig>();
var baseAddress = new Uri(grpcConfig?.BaseAddress ?? "https://localhost:7084");

builder.Services.AddGrpcClient<AuthenticationService.AuthenticationServiceClient>(options =>
{
    options.Address = baseAddress;
});

builder.Services.AddGrpcClient<AppointmentService.AppointmentServiceClient>(options =>
{
    options.Address = baseAddress;
});

builder.Services.AddGrpcClient<DoctorService.DoctorServiceClient>(options => { options.Address = baseAddress; });

builder.Services.AddGrpcClient<PatientService.PatientServiceClient>(options => { options.Address = baseAddress; });

builder.Services.AddGrpcClient<MedicalRecordService.MedicalRecordServiceClient>(options =>
{
    options.Address = baseAddress;
});

// Service implementations
builder.Services.AddScoped<IAuthenticationService, AuthenticationServiceGrpc>();
builder.Services.AddScoped<IAppointmentService, AppointmentServiceGrpc>();
builder.Services.AddScoped<IDoctorService, DoctorServiceGrpc>();
builder.Services.AddScoped<IPatientService, PatientServiceGrpc>();
builder.Services.AddScoped<IMedicalRecordService, MedicalRecordServiceGrpc>();


try
{
    Log.Information("Starting Medical Client web application");
    var app = builder.Build();

    // Configure pipeline
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