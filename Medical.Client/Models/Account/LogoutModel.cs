using Medical.Client.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using IAuthenticationService = Medical.Client.Interfaces.IAuthenticationService;

namespace Medical.Client.Models.Account;

public class LogoutModel : PageModel
{
    private readonly IAuthenticationService _authService;
    private readonly ITokenStorageService _tokenStorage;
    private readonly ILogger<LogoutModel> _logger;

    public LogoutModel(
        IAuthenticationService authService,
        ITokenStorageService tokenStorage,
        ILogger<LogoutModel> logger)
    {
        _authService = authService;
        _tokenStorage = tokenStorage;
        _logger = logger;
    }

    public async Task<IActionResult> OnPost()
    {
        try
        {
            var token = _tokenStorage.GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                await _authService.LogoutAsync(token);
                _tokenStorage.ClearToken();
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return RedirectToPage("/Error");
        }
    }
}