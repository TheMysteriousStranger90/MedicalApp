using System.Security.Claims;
using Medical.Client.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using IAuthenticationService = Medical.Client.Interfaces.IAuthenticationService;

namespace Medical.Client.Models.Account;

public class RegisterModel : PageModel
{
    private readonly IAuthenticationService _authService;
    private readonly ITokenStorageService _tokenStorage;
    private readonly ILogger<RegisterModel> _logger;

    [BindProperty] public RegisterInputModel Input { get; set; }
    public string ErrorMessage { get; set; }

    public RegisterModel(
        IAuthenticationService authService,
        ITokenStorageService tokenStorage,
        ILogger<RegisterModel> logger)
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

            var response = await _authService.RegisterAsync(Input);

            if (response?.Success == true)
            {
                _tokenStorage.SetToken(response.Token);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, response.UserId),
                    new Claim(ClaimTypes.Email, response.Email),
                    new Claim(ClaimTypes.Name, response.Email)
                };

                claims.AddRange(response.Roles.Select(role =>
                    new Claim(ClaimTypes.Role, role)));

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToPage("/Index");
            }

            ErrorMessage = response?.Message ?? "Registration failed.";
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user {Email}", Input.Email);
            ErrorMessage = "An error occurred during registration.";
            return Page();
        }
    }
}