using Medical.GrpcService.Context;
using Medical.GrpcService.Extensions;
using Medical.GrpcService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddGrpc();
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

app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<AppointmentGrpcService>();
app.MapGrpcService<DoctorGrpcService>();       
app.MapGrpcService<PatientGrpcService>();      
app.MapGrpcService<MedicalRecordGrpcService>();

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");


app.Run();