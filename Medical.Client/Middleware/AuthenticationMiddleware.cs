using Medical.Client.Interfaces;

namespace Medical.Client.Middleware;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ITokenStorageService _tokenStorage;

    public AuthenticationMiddleware(RequestDelegate next, ITokenStorageService tokenStorage)
    {
        _next = next;
        _tokenStorage = tokenStorage;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var token = _tokenStorage.GetToken();
        if (!string.IsNullOrEmpty(token))
        {
            context.Request.Headers.Add("Authorization", $"Bearer {token}");
        }

        await _next(context);
    }
}