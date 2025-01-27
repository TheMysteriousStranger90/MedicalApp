using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Medical.Client.Interceptors;

public class AuthInterceptor : Interceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
        if (!string.IsNullOrEmpty(token))
        {
            var metadata = new Metadata
            {
                { "Authorization", token }
            };
            var callOptions = context.Options.WithHeaders(metadata);
            context = new ClientInterceptorContext<TRequest, TResponse>(
                context.Method,
                context.Host,
                callOptions);
        }

        return continuation(request, context);
    }
}