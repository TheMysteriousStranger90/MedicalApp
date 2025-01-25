using Medical.GrpcService.Extensions;
using Medical.GrpcService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline
//
app.MapGrpcService<AppointmentGrpcService>();
app.MapGrpcService<DoctorGrpcService>();       
app.MapGrpcService<PatientGrpcService>();      
app.MapGrpcService<MedicalRecordGrpcService>();

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");


app.Run();