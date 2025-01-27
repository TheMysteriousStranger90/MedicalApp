using Medical.Client.Extensions;
using Medical.Client.Interceptors;
using Medical.Client.Interfaces;
using Medical.Client.Middleware;
using Medical.Client.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Core services
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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Authentication and storage
builder.Services.AddSingleton<ITokenStorageService, LocalStorageTokenService>();
builder.Services.AddScoped<AuthInterceptor>();

// gRPC clients
builder.Services.AddGrpcClients(builder.Configuration);

// Service implementations
builder.Services.AddScoped<IAuthenticationService, AuthenticationServiceGrpc>();
builder.Services.AddScoped<IAppointmentService, AppointmentServiceGrpc>();
builder.Services.AddScoped<IDoctorService, DoctorServiceGrpc>();
builder.Services.AddScoped<IPatientService, PatientServiceGrpc>();
builder.Services.AddScoped<IMedicalRecordService, MedicalRecordServiceGrpc>();

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