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
}