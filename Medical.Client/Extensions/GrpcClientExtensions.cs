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

        services.AddGrpcClient<AppointmentService.AppointmentServiceClient>((services, options) =>
            {
                options.Address = new Uri(grpcConfig?.BaseAddress ?? "https://localhost:7084");
            })
            .ConfigureChannel(options =>
            {
                options.HttpHandler = new SocketsHttpHandler
                {
                    EnableMultipleHttp2Connections = true,
                    KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                    KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
                    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1)
                };
            })
            .AddCallCredentials((context, metadata, serviceProvider) =>
            {
                var tokenService = serviceProvider.GetRequiredService<ITokenStorageService>();
                var token = tokenService.GetToken();
                if (!string.IsNullOrEmpty(token))
                {
                    if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        token = $"Bearer {token}";
                    }

                    metadata.Add("Authorization", token);
                }

                return Task.CompletedTask;
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