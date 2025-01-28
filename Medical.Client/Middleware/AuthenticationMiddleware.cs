using Medical.Client.Interfaces;

namespace Medical.Client.Middleware;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ITokenStorageService _tokenStorage;
    private readonly ILogger<AuthenticationMiddleware> _logger;

    public AuthenticationMiddleware(
        RequestDelegate next, 
        ITokenStorageService tokenStorage,
        ILogger<AuthenticationMiddleware> logger)
    {
        _next = next;
        _tokenStorage = tokenStorage;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var token = _tokenStorage.GetToken();
        _logger.LogDebug("Processing request: {Path}, Token exists: {HasToken}", 
            context.Request.Path, !string.IsNullOrEmpty(token));

        if (!string.IsNullOrEmpty(token))
        {
            if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = $"Bearer {token}";
            }
            context.Request.Headers["Authorization"] = token;
        }

        await _next(context);
    }
}