using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InsuranceProxy.Services;

public class JwtTokenService
{
    private readonly string _issuer;

    public JwtTokenService(string issuer)
    {
        _issuer = issuer;
    }

    public string GenerateToken(string audience, string secretKey)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: audience,
            claims: new[] { new Claim("role", "proxy") },
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}