using Medical.Client.Interfaces;

namespace Medical.Client.Services;

public class LocalStorageTokenService : ITokenStorageService
{
    private const string TokenKey = "jwt_token";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LocalStorageTokenService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetToken()
    {
        return _httpContextAccessor.HttpContext?.Request.Cookies[TokenKey];
    }

    public void SetToken(string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        };

        _httpContextAccessor.HttpContext?.Response.Cookies.Append(TokenKey, token, cookieOptions);
    }

    public void ClearToken()
    {
        _httpContextAccessor.HttpContext?.Response.Cookies.Delete(TokenKey);
    }
}