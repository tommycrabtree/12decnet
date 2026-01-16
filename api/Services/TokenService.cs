using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using api.Entities;
using api.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace api.Services;

public class TokenService : ITokenService
{
    public string CreateToken(AppUser user)
    {
        var tokenKey = Environment.GetEnvironmentVariable("TOKEN_KEY") 
                       ?? throw new Exception("Can't get token key, bud");

        if (tokenKey.Length < 64)
            throw new Exception("Token key needs to be at least 64 characters, boss");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));

        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id)
        };

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
    }
}
