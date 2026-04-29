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
            Console.WriteLine($"INPUT: {credentials.Username} / {credentials.Password}");
            Console.WriteLine($"CONFIG: {_config["Admin_Username"]} / {_config["Admin_Password"]}");

            if (credentials.Username != _config["Admin_Username"] || credentials.Password != _config["Admin_Password"])
            {
                return null;
            }

            // 🔑 2. Create claims (data inside JWT)
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, credentials.Username),
                new Claim(ClaimTypes.Role, "Admin")
            };

            // 🔐 3. Create signing key
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["JWT:Key"]!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 🧾 4. Create token
            var token = new JwtSecurityToken(
                issuer: _config["JWT:Issuer"],
                audience: _config["JWT:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(12),
                signingCredentials: creds
            );

            // 📦 5. Return token as string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
