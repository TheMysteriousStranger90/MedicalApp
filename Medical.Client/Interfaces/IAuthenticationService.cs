namespace Medical.Client.Interfaces;

public interface IAuthenticationService
{
    Task<LoginResponse> LoginAsync(string email, string password);
}