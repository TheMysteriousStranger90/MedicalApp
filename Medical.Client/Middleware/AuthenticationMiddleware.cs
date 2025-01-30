using System.Security.Claims;
using Medical.Client.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

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
        try 
        {
            var user = context.User;
            var token = _tokenStorage.GetToken();

            _logger.LogInformation(
                "User authenticated: {IsAuthenticated}, Role: {Role}, Path: {Path}", 
                user.Identity?.IsAuthenticated,
                string.Join(",", user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value)),
                context.Request.Path);

            if (!string.IsNullOrEmpty(token))
            {
                if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    token = $"Bearer {token}";
                }
                context.Request.Headers["Authorization"] = token;
            }
            else if (user.Identity?.IsAuthenticated == true)
            {
                _logger.LogWarning("Authenticated user without token, signing out");
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                context.Response.Redirect("/Account/Login");
                return;
            }

            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in authentication middleware");
            throw;
        }
    }
}