using Medical.GrpcService.Context;
using Medical.GrpcService.Extensions;
using Medical.GrpcService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(7084, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
        listenOptions.UseHttps("server.pfx", "P@ssw0rd!");
    });
});

// Add services to the container.
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddGrpc(options => { options.EnableDetailedErrors = true; });
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowGrpcWeb", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
    });
});

var app = builder.Build();

try
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.MigrateAndCreateDataAsync(scope.ServiceProvider);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while migrating or seeding the database");
    throw;
}

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseCors("AllowGrpcWeb");
app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });
app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<AuthenticationGrpcService>().EnableGrpcWeb();
app.MapGrpcService<AppointmentGrpcService>().EnableGrpcWeb();
app.MapGrpcService<DoctorGrpcService>().EnableGrpcWeb();
app.MapGrpcService<PatientGrpcService>().EnableGrpcWeb();
app.MapGrpcService<MedicalRecordGrpcService>().EnableGrpcWeb();

app.MapHealthChecks("/health");

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

app.MapGet("/version", () => new
{
    Version = "1.0.0",
    Environment = app.Environment.EnvironmentName
});

app.MapGet("/docs", () => Results.Redirect("https://github.com/TheMysteriousStranger90/MedicalApp"));


app.Run();