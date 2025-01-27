using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Medical.GrpcService.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Medical.GrpcService.Token;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly SymmetricSecurityKey _key;
    private readonly UserManager<User> _userManager;

    public TokenService(IConfiguration config, UserManager<User> userManager)
    {
        _config = config;
        _userManager = userManager;
        
        var keyBytes = Encoding.UTF8.GetBytes(_config["Token:Key"] 
                                              ?? throw new InvalidOperationException("Token:Key not configured"));
            
        if (keyBytes.Length * 8 < 512)
        {
            throw new InvalidOperationException(
                "Token key must be at least 512 bits (64 bytes) for HMAC-SHA512");
        }
        
        _key = new SymmetricSecurityKey(keyBytes);
    }

    public async Task<string> CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? throw new InvalidOperationException()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? throw new InvalidOperationException()),
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
        };

        var roles = await _userManager.GetRolesAsync(user);

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(7),
            SigningCredentials = creds,
            Issuer = _config["Token:Issuer"],
            Audience = _config["Token:Audience"],
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}