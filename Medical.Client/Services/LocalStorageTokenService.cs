using Medical.Client.Interfaces;

namespace Medical.Client.Services;

public class LocalStorageTokenService : ITokenStorageService
{
    private const string TokenKey = "jwt_token";
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<LocalStorageTokenService> _logger;

    public LocalStorageTokenService(
        IHttpContextAccessor httpContextAccessor,
        ILogger<LocalStorageTokenService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public string? GetToken()
    {
        var token = _httpContextAccessor.HttpContext?.Request.Cookies[TokenKey];
        _logger.LogDebug("Retrieved token from cookies: {TokenExists}", !string.IsNullOrEmpty(token));
        return token;
    }

    public void SetToken(string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(7)
        };

        _httpContextAccessor.HttpContext?.Response.Cookies.Append(TokenKey, token, cookieOptions);
        _logger.LogDebug("Token stored in cookies");
    }

    public void ClearToken()
    {
        _httpContextAccessor.HttpContext?.Response.Cookies.Delete(TokenKey);
    }
}