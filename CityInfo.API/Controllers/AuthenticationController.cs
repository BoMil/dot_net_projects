using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace CityInfo.API.Controllers
{
    [Route("api/authentication")]
    public class AuthenticationController : Controller
    {
        public class AuthenticationRequestBody
        {
            public string? Username { get; set; }
            public string? Password { get; set; }
        }
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IConfiguration _configuration;


        public AuthenticationController(ILogger<AuthenticationController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        }

        [HttpPost("authenticate")]
        public async Task<ActionResult<string>> Authenticate([FromBody] AuthenticationRequestBody request)
        {
            // Validate username and password
            var user = ValidateUserCredentials(request.Username, request.Password);

            if (user == null)
            {
                return Unauthorized("Invalid username or password");
            }
            else
            {
                // Create a JWT token
                var securityKey = new SymmetricSecurityKey(Convert.FromBase64String(_configuration["Authentication:SecretForKey"] ?? ""));
                var signInCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                // The claims collection contains the claims that will be serialized into the JWT
                var claims = new List<Claim>
                {
                    new Claim("sub", user.UserId.ToString()),
                    new Claim("given_name", user.FirstName),
                    new Claim("family_name", user.LastName),
                    new Claim("city", user.City)
                };
                var token = new JwtSecurityToken(
                    issuer: _configuration["Authentication:Issuer"],
                    audience: _configuration["Authentication:Audience"],
                    claims: claims,
                    notBefore: DateTime.UtcNow,
                    expires: DateTime.UtcNow.AddMinutes(30),
                    signingCredentials: signInCredentials);

                var tokenToReturn = new JwtSecurityTokenHandler().WriteToken(token);
                return Ok(tokenToReturn);
            }

        }

        private CityInfoUser ValidateUserCredentials(string? username, string? password)
        {
            // We don't have a user database, so we'll just return a hard-coded user and asume that creddentials are valid
            return new CityInfoUser(
                1,
                "Bojaca",
                "Boki",
                "Bole",
                "Antwerp"
            );
        }

        private class CityInfoUser
        {
            public int UserId { get; set; }
            public string UserName { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string City { get; set; }
            public CityInfoUser(
              int userId,
              string userName,
              string firstName,
              string lastName,
              string city
          )
            {
                UserId = userId;
                UserName = userName;
                FirstName = firstName;
                LastName = lastName;
                City = city;
            }
        }
    }
}