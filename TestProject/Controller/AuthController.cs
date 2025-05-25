using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static TestProject.Model.LoginModel.AuthClass;

namespace TestProject.Controller
{
    
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly JwtSettings _jwtSettings;

        public AuthController(IOptions<JwtSettings> jwtOptions)
        {
            _jwtSettings = jwtOptions.Value;
        }
        [Route("api/Auth/login")]
        [HttpPost]
        public IActionResult Login(LoginRequest request)
        {
            if (request.Username == "admin" && request.Password == "password123")
            {
                string sessionId = Guid.NewGuid().ToString();
                var token = GenerateToken(sessionId);
                return Ok(new { token });
            }
            return Unauthorized("Invalid credentials");
        }
        private string GenerateToken(string sessionId)
        {
            var claims = new[]
            {
            new Claim("sessionId", sessionId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}