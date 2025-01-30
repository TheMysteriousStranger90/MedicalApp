using Medical.Client.Configuration;
using Medical.Client.Interceptors;
using Medical.Client.Interfaces;

namespace Medical.Client.Extensions;

public static class GrpcClientExtensions
{
    public static IServiceCollection AddGrpcClients(this IServiceCollection services, IConfiguration configuration)
    {
        var grpcConfig = configuration.GetSection("GrpcClient").Get<GrpcClientConfig>();
        var baseAddress = new Uri(grpcConfig?.BaseAddress ?? "https://localhost:7084");

        var handler = new SocketsHttpHandler
        {
            EnableMultipleHttp2Connections = true,
            KeepAlivePingDelay = TimeSpan.FromSeconds(60),
            KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
            SslOptions = new System.Net.Security.SslClientAuthenticationOptions
            {
                RemoteCertificateValidationCallback = (sender, certificate, chain, errors) => true
            }
        };
        
        AddGrpcClient<AppointmentService.AppointmentServiceClient>(services, baseAddress, handler);
        AddGrpcClient<DoctorService.DoctorServiceClient>(services, baseAddress, handler);
        AddGrpcClient<PatientService.PatientServiceClient>(services, baseAddress, handler);
        AddGrpcClient<MedicalRecordService.MedicalRecordServiceClient>(services, baseAddress, handler);
        AddGrpcClient<AuthenticationService.AuthenticationServiceClient>(services, baseAddress, handler);

        return services;
    }

    private static void AddGrpcClient<TClient>(IServiceCollection services, Uri baseAddress, SocketsHttpHandler handler) 
        where TClient : class
    {
        services.AddGrpcClient<TClient>(options =>
            {
                options.Address = baseAddress;
            })
            .ConfigureChannel(options =>
            {
                options.HttpHandler = handler;
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
            })
            .AddInterceptor<AuthInterceptor>();
    }
}
