using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Medical.GrpcService.Extensions;

public static class OpenTelemetryExtensions
{
    private const string ServiceName = "Medical.GrpcService";
    private const string ServiceVersion = "1.1.0";

    public static IServiceCollection AddOpenTelemetryObservability(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string? otlpEndpoint = configuration["OpenTelemetry:OtlpEndpoint"];

        ResourceBuilder resourceBuilder = ResourceBuilder
            .CreateDefault()
            .AddService(ServiceName, serviceVersion: ServiceVersion)
            .AddTelemetrySdk()
            .AddEnvironmentVariableDetector();

        services
            .AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation(opts =>
                    {
                        opts.Filter = ctx =>
                            // Skip health probes and Prometheus scraping from traces
                            !ctx.Request.Path.StartsWithSegments("/health") &&
                            !ctx.Request.Path.StartsWithSegments("/metrics") &&
                            // Skip gRPC reflection (dev tooling noise)
                            !ctx.Request.Path.StartsWithSegments("/grpc.reflection");
                    })
                    .AddHttpClientInstrumentation();

                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                    tracing.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
                else
                    tracing.AddConsoleExporter();
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddPrometheusExporter();

                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                    metrics.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
            });

        return services;
    }
}
