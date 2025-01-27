using System.Security.Claims;
using Medical.Client.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using IAuthenticationService = Medical.Client.Interfaces.IAuthenticationService;

namespace Medical.Client.Models.Account;

public class LoginModel : PageModel
{
    private readonly IAuthenticationService _authService;
    private readonly ITokenStorageService _tokenStorage;
    private readonly ILogger<LoginModel> _logger;

    [BindProperty] public LoginInputModel Input { get; set; }
    public string ErrorMessage { get; set; }

    public LoginModel(
        IAuthenticationService authService,
        ITokenStorageService tokenStorage,
        ILogger<LoginModel> logger)
    {
        _authService = authService;
        _tokenStorage = tokenStorage;
        _logger = logger;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            if (!ModelState.IsValid)
                return Page();

            var response = await _authService.LoginAsync(Input.Email, Input.Password);

            if (response?.Token != null)
            {
                _tokenStorage.SetToken(response.Token);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, response.Email),
                    new Claim(ClaimTypes.Name, response.Email),
                    new Claim(ClaimTypes.NameIdentifier, response.UserId),
                };

                claims.AddRange(response.Roles.Select(role =>
                    new Claim(ClaimTypes.Role, role)));

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                return RedirectToPage("/Index");
            }

            ErrorMessage = "Invalid login attempt.";
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Email}", Input.Email);
            ErrorMessage = "An error occurred during login.";
            return Page();
        }
    }
}