using Medical.Client.Configuration;
using Medical.Client.Interceptors;

namespace Medical.Client.Extensions;

public static class GrpcClientExtensions
{
    public static IServiceCollection AddGrpcClients(this IServiceCollection services,
        IConfiguration configuration)
    {
        var grpcConfig = configuration.GetSection("GrpcClient").Get<GrpcClientConfig>();

        services.AddGrpcClient<AuthenticationService.AuthenticationServiceClient>((services, options) =>
        {
            options.Address = new Uri(grpcConfig?.BaseAddress ?? "https://localhost:7084");
        }).AddInterceptor<AuthInterceptor>();

        services.AddGrpcClient<AppointmentService.AppointmentServiceClient>((services, options) =>
        {
            options.Address = new Uri(grpcConfig?.BaseAddress ?? "https://localhost:7084");
        }).AddInterceptor<AuthInterceptor>();

        services.AddGrpcClient<DoctorService.DoctorServiceClient>((services, options) =>
        {
            options.Address = new Uri(grpcConfig?.BaseAddress ?? "https://localhost:7084");
        }).AddInterceptor<AuthInterceptor>();

        services.AddGrpcClient<PatientService.PatientServiceClient>((services, options) =>
        {
            options.Address = new Uri(grpcConfig?.BaseAddress ?? "https://localhost:7084");
        }).AddInterceptor<AuthInterceptor>();

        services.AddGrpcClient<MedicalRecordService.MedicalRecordServiceClient>((services, options) =>
        {
            options.Address = new Uri(grpcConfig?.BaseAddress ?? "https://localhost:7084");
        }).AddInterceptor<AuthInterceptor>();

        return services;
    }
}