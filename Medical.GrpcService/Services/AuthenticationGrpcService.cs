﻿using Grpc.Core;
using Medical.GrpcService.Entities;
using Medical.GrpcService.Entities.Enums;
using Medical.GrpcService.Token;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;

namespace Medical.GrpcService.Services;

public class AuthenticationGrpcService : AuthenticationService.AuthenticationServiceBase
{
    private const string DOCTOR_EMAIL_DOMAIN = "medicalapp";
    private const string DOCTOR_ROLE = "Doctor";
    private const string PATIENT_ROLE = "Patient";

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

            var emailParts = request.Email.Split('@');
            var isDoctorEmail = emailParts.Length == 2 && 
                                emailParts[1].StartsWith(DOCTOR_EMAIL_DOMAIN, StringComparison.OrdinalIgnoreCase);
        
            User user;
            if (isDoctorEmail)
            {
                user = new Doctor
                {
                    UserName = request.Email,
                    Email = request.Email,
                    FullName = request.FullName ?? "Doctor",
                    Specialization = request.Specialization ?? "General",
                    LicenseNumber = request.LicenseNumber ?? "TBD",
                    ConsultationFee = (decimal)(request.ConsultationFee > 0 ? request.ConsultationFee : 50),
                    Created = DateTime.UtcNow,
                    LastActive = DateTime.UtcNow,
                    IsActive = true
                };
            }
            else
            {
                user = new Patient
                {
                    UserName = request.Email,
                    Email = request.Email,
                    FullName = request.FullName ?? "Patient",
                    DateOfBirth = request.DateOfBirth?.ToDateTime() ?? DateTime.UtcNow,
                    Gender = Enum.TryParse<Gender>(request.Gender, true, out var gender) ? gender : Gender.Male,
                    Phone = request.Phone ?? string.Empty,
                    Address = request.Address ?? string.Empty,
                    Created = DateTime.UtcNow,
                    LastActive = DateTime.UtcNow,
                    IsActive = true
                };
            }

            var role = isDoctorEmail ? DOCTOR_ROLE : PATIENT_ROLE;
            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, 
                    string.Join(", ", result.Errors.Select(e => e.Description))));
            }

            if (!await _roleManager.RoleExistsAsync(role))
            {
                var roleResult = await _roleManager.CreateAsync(new Role { Name = role });
                if (!roleResult.Succeeded)
                {
                    _logger.LogError("Failed to create role: {Role}", role);
                    throw new RpcException(new Status(StatusCode.Internal, "Failed to create role"));
                }
            }
            
            var roleAssignResult = await _userManager.AddToRoleAsync(user, role);
            if (!roleAssignResult.Succeeded)
            {
                _logger.LogError("Failed to assign role {Role} to user {Email}", role, user.Email);
                throw new RpcException(new Status(StatusCode.Internal, "Failed to assign role"));
            }

            _logger.LogInformation("User {Email} registered successfully with role {Role}", user.Email, role);

            var token = await _tokenService.CreateToken(user);
            var roles = await _userManager.GetRolesAsync(user);

            return new RegisterResponse
            {
                Success = true,
                Message = $"Registration successful as {role}",
                Token = token,
                Email = user.Email,
                Roles = { roles },
                UserId = user.Id
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