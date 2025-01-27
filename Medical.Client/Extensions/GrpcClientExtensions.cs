using Medical.Client.Configuration;
using Medical.Client.Interceptors;
using Medical.Client.Interfaces;

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
        })
        .AddCallCredentials(async (context, metadata, serviceProvider) =>
        {
            var tokenService = serviceProvider.GetRequiredService<ITokenStorageService>();
            var token = tokenService.GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                metadata.Add("Authorization", $"Bearer {token}");
            }
        });

        services.AddGrpcClient<AppointmentService.AppointmentServiceClient>((services, options) =>
        {
            options.Address = new Uri(grpcConfig?.BaseAddress ?? "https://localhost:7084");
        })
        .AddCallCredentials(async (context, metadata, serviceProvider) =>
        {
            var tokenService = serviceProvider.GetRequiredService<ITokenStorageService>();
            var token = tokenService.GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                metadata.Add("Authorization", $"Bearer {token}");
            }
        })
        .AddInterceptor<AuthInterceptor>();

        services.AddGrpcClient<DoctorService.DoctorServiceClient>((services, options) =>
        {
            options.Address = new Uri(grpcConfig?.BaseAddress ?? "https://localhost:7084");
        })
        .AddCallCredentials(async (context, metadata, serviceProvider) =>
        {
            var tokenService = serviceProvider.GetRequiredService<ITokenStorageService>();
            var token = tokenService.GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                metadata.Add("Authorization", $"Bearer {token}");
            }
        })
        .AddInterceptor<AuthInterceptor>();

        services.AddGrpcClient<PatientService.PatientServiceClient>((services, options) =>
        {
            options.Address = new Uri(grpcConfig?.BaseAddress ?? "https://localhost:7084");
        })
        .AddCallCredentials(async (context, metadata, serviceProvider) =>
        {
            var tokenService = serviceProvider.GetRequiredService<ITokenStorageService>();
            var token = tokenService.GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                metadata.Add("Authorization", $"Bearer {token}");
            }
        })
        .AddInterceptor<AuthInterceptor>();

        services.AddGrpcClient<MedicalRecordService.MedicalRecordServiceClient>((services, options) =>
        {
            options.Address = new Uri(grpcConfig?.BaseAddress ?? "https://localhost:7084");
        })
        .AddCallCredentials(async (context, metadata, serviceProvider) =>
        {
            var tokenService = serviceProvider.GetRequiredService<ITokenStorageService>();
            var token = tokenService.GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                metadata.Add("Authorization", $"Bearer {token}");
            }
        })
        .AddInterceptor<AuthInterceptor>();

        return services;
    }
}