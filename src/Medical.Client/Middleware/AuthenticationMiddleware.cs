using System.Security.Claims;
using Medical.Client.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Medical.Client.Middleware;

public class AuthenticationMiddleware
{
    private static readonly string[] _staticExtensions =
        [".css", ".js", ".png", ".jpg", ".jpeg", ".gif", ".svg", ".ico", ".woff", ".woff2", ".map"];

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
        // Skip middleware overhead for static file requests
        var path = context.Request.Path.Value ?? string.Empty;
        if (IsStaticFile(path))
        {
            await _next(context);
            return;
        }

        var user = context.User;
        var token = _tokenStorage.GetToken();

        _logger.LogDebug(
            "Auth check — Authenticated: {IsAuthenticated}, Roles: [{Roles}], Path: {Path}",
            user.Identity?.IsAuthenticated,
            string.Join(", ", user.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)),
            context.Request.Path);

        if (user.Identity?.IsAuthenticated == true && string.IsNullOrEmpty(token))
        {
            _logger.LogWarning(
                "Authenticated session found with no JWT token — signing out user");

            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            context.Response.Redirect("/Account/Login");
            return;
        }

        await _next(context);
    }

    private static bool IsStaticFile(string path) =>
        _staticExtensions.Any(ext =>
            path.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
}
