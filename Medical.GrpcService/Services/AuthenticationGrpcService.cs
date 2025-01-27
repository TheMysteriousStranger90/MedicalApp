using Grpc.Core;
using Medical.GrpcService.Entities;
using Medical.GrpcService.Token;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;

namespace Medical.GrpcService.Services;

public class AuthenticationGrpcService : AuthenticationService.AuthenticationServiceBase
{
    private const string DEFAULT_ROLE = "Patient";
    
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly RoleManager<Role> _roleManager;
    private readonly ILogger<AuthenticationGrpcService> _logger;

    public AuthenticationGrpcService(
        UserManager<User> userManager,
        ITokenService tokenService,
        RoleManager<Role> roleManager,
        ILogger<AuthenticationGrpcService> logger)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _roleManager = roleManager;
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
                Roles = { roles },
                UserId = user.Id,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Email}", request.Email);
            throw new RpcException(new Status(StatusCode.Internal, "Login failed"));
        }
    }

    public override async Task<RegisterResponse> Register(RegisterRequest request, ServerCallContext context)
    {
        try
        {
            if (await _userManager.FindByEmailAsync(request.Email) != null)
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, "Email already registered"));
            }

            var user = new User
            {
                UserName = request.Email,
                Email = request.Email,
                Created = DateTime.UtcNow,
                LastActive = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, 
                    string.Join(", ", result.Errors.Select(e => e.Description))));
            }

            // Ensure Patient role exists and assign it
            if (!await _roleManager.RoleExistsAsync(DEFAULT_ROLE))
            {
                await _roleManager.CreateAsync(new Role { Name = DEFAULT_ROLE });
            }

            await _userManager.AddToRoleAsync(user, DEFAULT_ROLE);

            // Generate token
            var token = await _tokenService.CreateToken(user);
            var roles = await _userManager.GetRolesAsync(user);

            return new RegisterResponse
            {
                Success = true,
                Message = "Registration successful",
                Token = token,
                Email = user.Email,
                Roles = { roles },
                UserId = user.Id,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user {Email}", request.Email);
            throw new RpcException(new Status(StatusCode.Internal, "Registration failed"));
        }
    }

    public override Task<LogoutResponse> Logout(LogoutRequest request, ServerCallContext context)
    {
        return Task.FromResult(new LogoutResponse
        {
            Success = true,
            Message = "Logged out successfully"
        });
    }
}