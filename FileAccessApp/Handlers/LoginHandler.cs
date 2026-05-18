using FileAccessApp.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FileAccessApp.Handlers
{
    public class LoginHandler
    {
        private readonly IConfiguration _config;
        public LoginHandler(IConfiguration configuration)
        {
            _config = configuration;
        }

        public string? Handle(Credentials credentials)
        {
            if (credentials.Username != _config["Admin_Username"] || credentials.Password != _config["Admin_Password"])
            {
                return null;
            }

            // Create claims (data inside JWT)
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, credentials.Username),
                new Claim(ClaimTypes.Role, "Admin")
            };

            // Create signing key
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["JWT:Key"]!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Create token
            var token = new JwtSecurityToken(
                issuer: _config["JWT:Issuer"],
                audience: _config["JWT:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
