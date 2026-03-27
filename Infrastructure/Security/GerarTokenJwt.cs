

using GestaoPedidos.Domain.Abstractions.Usuarios;
using GestaoPedidos.Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GestaoPedidos.Infrastructure.Security
{
    public class GerarTokenJwt : IToken
    {
        private readonly JwtSettings _jwtSettings;


        public GerarTokenJwt (JwtSettings jwtSettings)
        {
            _jwtSettings = jwtSettings;
        }

        public string GerarToken(Usuario usuario)
        {
            var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Email),
                new Claim("nome", usuario.Nome),
                new Claim(ClaimTypes.Role, usuario.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(_jwtSettings.ExpiracaoHoras),
                signingCredentials: credenciais);

            return new JwtSecurityTokenHandler().WriteToken(token); 
        }

    }


    public class JwtSettings
    {
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpiracaoHoras { get; set; } = 8;
    }
}
