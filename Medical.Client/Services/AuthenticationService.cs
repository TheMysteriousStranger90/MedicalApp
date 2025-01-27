using Grpc.Core;
using Medical.Client.Interfaces;

namespace Medical.Client.Services;

public class AuthenticationServiceGrpc : IAuthenticationService
{
    private readonly Medical.Client.AuthenticationService.AuthenticationServiceClient _client;
    private readonly ILogger<AuthenticationServiceGrpc> _logger;

    public AuthenticationServiceGrpc(
        Medical.Client.AuthenticationService.AuthenticationServiceClient client,
        ILogger<AuthenticationServiceGrpc> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<LoginResponse> LoginAsync(string email, string password)
    {
        try
        {
            var request = new LoginRequest { Email = email, Password = password };
            return await _client.LoginAsync(request);
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error during login for user {Email}", email);
            throw;
        }
    }
    
    public async Task<RegisterResponse> RegisterAsync(string email, string password, string fullName)
    {
        try
        {
            var request = new RegisterRequest 
            { 
                Email = email, 
                Password = password,
                FullName = fullName
            };
            return await _client.RegisterAsync(request);
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error during registration for user {Email}", email);
            throw;
        }
    }

    public async Task<LogoutResponse> LogoutAsync(string token)
    {
        try
        {
            var request = new LogoutRequest { Token = token };
            return await _client.LogoutAsync(request);
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error during logout");
            throw;
        }
    }
}