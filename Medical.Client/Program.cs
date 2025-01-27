using Grpc.Net.Client;
using Medical.Client.Extensions;
using Medical.Client.Interceptors;
using Medical.Client.Interfaces;
using Medical.Client.Services;
using AuthenticationService = Medical.Client.AuthenticationService;
using DoctorService = Medical.Client.DoctorService;
using MedicalRecordService = Medical.Client.MedicalRecordService;
using PatientService = Medical.Client.PatientService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddGrpcClients(builder.Configuration);
builder.Services.AddScoped<AuthInterceptor>();


builder.Services.AddScoped<IAuthenticationService, AuthenticationServiceGrpc>();
builder.Services.AddScoped<IAppointmentService, AppointmentServiceGrpc>();
builder.Services.AddScoped<IDoctorService, DoctorServiceGrpc>();
builder.Services.AddScoped<IPatientService, PatientServiceGrpc>();
builder.Services.AddScoped<IMedicalRecordService, MedicalRecordServiceGrpc>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();