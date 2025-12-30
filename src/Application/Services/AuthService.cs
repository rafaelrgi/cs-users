using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Users.src.Domain.Contracts;
using Users.src.Domain.Entities;

namespace Users.src.Application.Services
{
  public class AuthService : IAuthService
  {
    const int VALIDITY_TOKEN_HOURS = 11;
    const string JWT_CONFIG_KEY = "PrivateKey";

    readonly IUserService _users;
    readonly IConfiguration _configuration;
    readonly ILogger<AuthService> _logger;

    public AuthService(IConfiguration configuration, IUserService users, ILogger<AuthService> logger)
    {
      _configuration = configuration;
      _users = users;
      _logger = logger;
    }

    public async Task<string> Login(string email, string password)
    {
      email = (email ?? "").Trim();
      var user = await _users.FindByEmail(email);
      if (user == null)
      {
        _logger.LogWarning($"Login: user not found {email}");
        return "";
      }

      if (!CheckPassword(password, user.Password))
      {
        _logger.LogWarning($"Login: wrong password for user {email}");
        return "";
      }

      var token = GenerateToken(user, VALIDITY_TOKEN_HOURS);
      return token;
    }

    public string GenerateToken(User user, int hoursValidity)
    {
      var tokenHandler = new JwtSecurityTokenHandler();
      var jwtKey = _configuration[JWT_CONFIG_KEY];

      if (string.IsNullOrWhiteSpace(jwtKey))
      {
        _logger.LogError($"Could no load Public Key from {JWT_CONFIG_KEY}");
        return "";
      }

      var rsa = RSA.Create();
      rsa.ImportFromPem(jwtKey.ToCharArray());
      var key = Encoding.ASCII.GetBytes(jwtKey);
      var tokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity(
          [
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            //new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.IsAdmin? "Admin" : "User")
          ]),
        Expires = DateTime.UtcNow.AddHours(hoursValidity),
        SigningCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256),
      };

      var token = tokenHandler.CreateToken(tokenDescriptor);
      return tokenHandler.WriteToken(token);
    }

    private static bool CheckPassword(string pass, string hash)
    {
      return BCrypt.Net.BCrypt.Verify(pass, hash);
    }

  }
}
