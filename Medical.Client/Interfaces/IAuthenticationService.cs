namespace Medical.Client.Interfaces;

public interface IAuthenticationService
{
    Task<LoginResponse> LoginAsync(string email, string password);
    Task<RegisterResponse> RegisterAsync(string email, string password, string fullName);
    Task<LogoutResponse> LogoutAsync(string token);
}