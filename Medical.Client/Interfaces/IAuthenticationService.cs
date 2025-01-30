using Medical.Client.Models;

namespace Medical.Client.Interfaces;

public interface IAuthenticationService
{
    Task<LoginResponse> LoginAsync(string email, string password);
    Task<RegisterResponse> RegisterAsync(RegisterInputModel input);
    Task<LogoutResponse> LogoutAsync(string token);
}