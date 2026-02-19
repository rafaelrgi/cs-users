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

    public async Task<(string token, User? user)> Login(string email, string password)
    {
      string token;
      email = (email ?? "").Trim();
      var user = await _users.FindByEmail(email);
      if (user == null)
      {
        _logger.LogWarning("Login: user not found {email}", email);
        return ("", null);
      }

      if (user.IsDeleted)
      {
        _logger.LogWarning("Login: user is blocked: {email}", email);
        return ("", null);
      }

      if (!CheckPassword(password, user.Password))
      {
        _logger.LogWarning("Login: wrong password for user {email}", email);
        return ("", null);
      }

      token = GenerateToken(user, VALIDITY_TOKEN_HOURS);
      return (token, user);
    }

    public string GenerateToken(User user, int hoursValidity)
    {
      var tokenHandler = new JwtSecurityTokenHandler();
      var keyPath = _configuration["RsaKeys:PrivateKeyPath"] ?? "";
      if (string.IsNullOrEmpty(keyPath) || !File.Exists(keyPath))
      {
        _logger.LogError("Private Key not found at: {Path}", keyPath);
        return string.Empty;
      }

      var pemContent = File.ReadAllText(keyPath);
      if (string.IsNullOrWhiteSpace(pemContent))
      {
        _logger.LogError($"Could no load Public Key");
        return string.Empty;
      }

      try
      {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(pemContent);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
          Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
            }),
          Expires = DateTime.UtcNow.AddHours(hoursValidity),

          SigningCredentials = new SigningCredentials(
                new RsaSecurityKey(rsa),
                SecurityAlgorithms.RsaSha256
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error: could not generate JWT token");
        return string.Empty;
      }
    }

    public static bool CheckPassword(string pass, string hash)
    {
      return BCrypt.Net.BCrypt.Verify(pass, hash);
    }

  }
}
