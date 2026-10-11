using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Cafeteria.Application.Interfaces;
using Cafeteria.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Cafeteria.Infrastructure.Services.Auth;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerarToken(Usuario usuario, out DateTime expiracion)
    {
        var secretKey = _configuration["Jwt:Key"] ?? "CafeteriaSuperSecretKeyParaFirmaDeTokensJWT2026!*SecureTokenKey";
        var issuer = _configuration["Jwt:Issuer"] ?? "CafeteriaApi";
        var audience = _configuration["Jwt:Audience"] ?? "CafeteriaApiUsers";
        var expirationHours = int.TryParse(_configuration["Jwt:ExpiresInHours"], out var hours) ? hours : 8;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        expiracion = DateTime.UtcNow.AddHours(expirationHours);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Username),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.Rol.ToString()),
            new("NombreCompleto", usuario.NombreCompleto),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiracion,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
