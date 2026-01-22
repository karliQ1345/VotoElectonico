using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VotoElectonico.Data;
using VotoElectonico.Models;
using VotoElectonico.Options;

namespace VotoElectonico.Services.Auth
{
    public class TokenService : ITokenService
    {
        private readonly ApplicationDbContext _db;
        private readonly JwtOptions _opt;

        public TokenService(ApplicationDbContext db, IOptions<JwtOptions> opt)
        {
            _db = db;
            _opt = opt.Value;
        }

        public async Task<string> CreateTokenAsync(Usuario user, CancellationToken ct)
        {
            var roles = await _db.UsuarioRoles
                .Where(r => r.UsuarioId == user.Id)
                .Select(r => r.Rol.ToString())
                .ToListAsync(ct);

            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim("cedula", user.Cedula),
            new Claim("nombre", user.NombreCompleto ?? "")
        };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _opt.Issuer,
                audience: _opt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_opt.ExpireMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}