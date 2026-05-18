using Medical.Client.Models;

namespace Medical.Client.Interfaces;

public interface IAuthenticationService
{
    public Task<LoginResponse> LoginAsync(string email, string password);
    public Task<RegisterResponse> RegisterAsync(RegisterInputModel input);
    public Task<LogoutResponse> LogoutAsync(string token);
}
