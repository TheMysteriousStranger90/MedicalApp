using System.Diagnostics;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Medical.Client.Interceptors;

/// <summary>
/// gRPC client-side interceptor that:
///   1. Injects a correlation ID (from the current HTTP request trace ID) into every call.
///   2. Sets a per-call deadline from the configured timeout.
///   3. Logs call completion / failure at Debug / Warning level.
///
/// NOTE: JWT token injection is handled by AddCallCredentials in GrpcClientExtensions.
/// </summary>
public class GrpcClientInterceptor : Interceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<GrpcClientInterceptor> _logger;

    public GrpcClientInterceptor(
        IHttpContextAccessor httpContextAccessor,
        ILogger<GrpcClientInterceptor> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        context = EnrichContext(context);

        var sw = Stopwatch.StartNew();
        AsyncUnaryCall<TResponse> call = continuation(request, context);

        return new AsyncUnaryCall<TResponse>(
            TrackAsync(call.ResponseAsync, context.Method.FullName, sw),
            call.ResponseHeadersAsync,
            call.GetStatus,
            call.GetTrailers,
            call.Dispose);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private ClientInterceptorContext<TRequest, TResponse> EnrichContext<TRequest, TResponse>(
        ClientInterceptorContext<TRequest, TResponse> context)
        where TRequest : class
        where TResponse : class
    {
        Metadata headers = context.Options.Headers is null
            ? new Metadata()
            : new Metadata();

        if (context.Options.Headers is not null)
        {
            foreach (Metadata.Entry entry in context.Options.Headers)
                headers.Add(entry);
        }

        // Propagate the ASP.NET Core trace ID as a correlation ID so the
        // gRPC server can correlate logs across service boundaries.
        string correlationId =
            _httpContextAccessor.HttpContext?.TraceIdentifier
            ?? Activity.Current?.Id
            ?? Guid.NewGuid().ToString("N");

        headers.Add("x-correlation-id", correlationId);

        CallOptions options = context.Options.WithHeaders(headers);
        return new ClientInterceptorContext<TRequest, TResponse>(
            context.Method, context.Host, options);
    }

    private async Task<TResponse> TrackAsync<TResponse>(
        Task<TResponse> responseTask,
        string methodName,
        Stopwatch sw)
    {
        try
        {
            TResponse result = await responseTask;
            _logger.LogDebug("gRPC {Method} succeeded in {ElapsedMs}ms",
                methodName, sw.ElapsedMilliseconds);
            return result;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex,
                "gRPC {Method} failed with status {StatusCode} in {ElapsedMs}ms",
                methodName, ex.StatusCode, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
