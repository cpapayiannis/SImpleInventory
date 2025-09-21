using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SimpleInventory.Authorization.Intefaces;
using SimpleInventory.Common.Classes;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SimpleInventory.Authorization.Classes;
public class AuthorizationClient : IAuthorizationClient
{
    private readonly JwtSettings _settings;

    public AuthorizationClient(IOptions<JwtSettings> options)
    {
        _settings = options.Value;
    }

    public string GenerateToken(string username)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(60), // token lifetime
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}