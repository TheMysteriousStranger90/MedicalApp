using Grpc.Core;
using Medical.GrpcService.Entities;
using Medical.GrpcService.Token;
using Microsoft.AspNetCore.Identity;

namespace Medical.GrpcService.Services;

public class AuthenticationGrpcService : AuthenticationService.AuthenticationServiceBase
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthenticationGrpcService> _logger;

    public AuthenticationGrpcService(
        UserManager<User> userManager,
        ITokenService tokenService,
        ILogger<AuthenticationGrpcService> logger)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _logger = logger;
    }

    public override async Task<LoginResponse> Login(LoginRequest request, ServerCallContext context)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "User not found"));
            }

            var result = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!result)
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid password"));
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = await _tokenService.CreateToken(user);

            return new LoginResponse
            {
                Token = token,
                Email = user.Email,
                Roles = { roles }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Email}", request.Email);
            throw new RpcException(new Status(StatusCode.Internal, "Login failed"));
        }
    }
}