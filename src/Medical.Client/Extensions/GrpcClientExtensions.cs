using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using Medical.Client.Configuration;
using Medical.Client.Interceptors;
using Medical.Client.Interfaces;

namespace Medical.Client.Extensions;

public static class GrpcClientExtensions
{
    public static IServiceCollection AddGrpcClients(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        GrpcClientConfig? grpcConfig = configuration.GetSection("GrpcClient").Get<GrpcClientConfig>();
        var baseAddress = new Uri(grpcConfig?.BaseAddress ?? "https://localhost:7084");
        var callTimeout = TimeSpan.FromSeconds(grpcConfig?.Timeout > 0 ? grpcConfig.Timeout : 30);

        var handler = new SocketsHttpHandler
        {
            EnableMultipleHttp2Connections = true,
            KeepAlivePingDelay = TimeSpan.FromSeconds(60),
            KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5)
        };

        // In development allow self-signed certificates (dev cert / makemedicalcerts.ps1 output).
        // In all other environments let the OS/trust-store validate the certificate.
        if (environment.IsDevelopment())
        {
#pragma warning disable CA5359 // RemoteCertificateValidationCallback — intentionally bypassed in Development only for self-signed certs
            handler.SslOptions = new System.Net.Security.SslClientAuthenticationOptions
            {
                RemoteCertificateValidationCallback = (_, _, _, _) => true
            };
#pragma warning restore CA5359
        }

        // gRPC built-in client-side retry — no extra packages needed.
        // Retries on Unavailable (server not ready) and DeadlineExceeded (transient timeout).
        var retryPolicy = new MethodConfig
        {
            Names = { MethodName.Default },
            RetryPolicy = new RetryPolicy
            {
                MaxAttempts = 3,
                InitialBackoff = TimeSpan.FromMilliseconds(500),
                MaxBackoff = TimeSpan.FromSeconds(5),
                BackoffMultiplier = 1.5,
                RetryableStatusCodes = { Grpc.Core.StatusCode.Unavailable }
            }
        };

        var serviceConfig = new ServiceConfig { MethodConfigs = { retryPolicy } };

        AddGrpcClient<AppointmentService.AppointmentServiceClient>(
            services, baseAddress, handler, serviceConfig, callTimeout);
        AddGrpcClient<DoctorService.DoctorServiceClient>(
            services, baseAddress, handler, serviceConfig, callTimeout);
        AddGrpcClient<PatientService.PatientServiceClient>(
            services, baseAddress, handler, serviceConfig, callTimeout);
        AddGrpcClient<MedicalRecordService.MedicalRecordServiceClient>(
            services, baseAddress, handler, serviceConfig, callTimeout);
        AddGrpcClient<AuthenticationService.AuthenticationServiceClient>(
            services, baseAddress, handler, serviceConfig, callTimeout);

        return services;
    }

    private static void AddGrpcClient<TClient>(
        IServiceCollection services,
        Uri baseAddress,
        SocketsHttpHandler handler,
        ServiceConfig serviceConfig,
        TimeSpan callTimeout)
        where TClient : class
    {
        services.AddGrpcClient<TClient>(options => { options.Address = baseAddress; })
            .ConfigureChannel(options =>
            {
                options.HttpHandler = handler;
                options.ServiceConfig = serviceConfig;
            })
            .AddCallCredentials((context, metadata, serviceProvider) =>
            {
                ITokenStorageService tokenService = serviceProvider.GetRequiredService<ITokenStorageService>();
                string? token = tokenService.GetToken();
                if (!string.IsNullOrEmpty(token))
                {
                    metadata.Add("Authorization",
                        token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                            ? token
                            : $"Bearer {token}");
                }

                return Task.CompletedTask;
            })
            .AddInterceptor<GrpcClientInterceptor>();
    }
}
