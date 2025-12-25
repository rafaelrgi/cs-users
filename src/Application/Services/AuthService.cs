using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Users.src.Domain.Contracts;
using Users.src.Domain.Entities;

namespace Users.src.Application.Services
{
  public class AuthService : IAuthService
  {
    const int VALIDITY_TOKEN_HOURS = 11;
    //using a property-like const so the "Program" can use it too
    public static string JWT_CONFIG_KEY { get => "Jwt:Key"; }

    readonly IConfiguration _configuration;
    readonly IUsersService _users;

    public AuthService(IConfiguration configuration, IUsersService users)
    {
      _configuration = configuration;
      _users = users;
    }

    public async Task<string> Login(string email, string password)
    {
      email = (email ?? "").Trim();
      var user = await _users.FindByEmail(email);
      if (user == null)
        return "";

      if (!CheckPassword(password, user.Password))
        return "";

      var token = GenerateToken(user, VALIDITY_TOKEN_HOURS);
      return token;
    }

    public string GenerateToken(User user, int hoursValidity)
    {
      var tokenHandler = new JwtSecurityTokenHandler();
      var jwtKey = _configuration[JWT_CONFIG_KEY];
      if (string.IsNullOrWhiteSpace(jwtKey))
        return "";

      var key = Encoding.ASCII.GetBytes(jwtKey);
      var tokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity(
          [
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            //new Claim(ClaimTypes.Email, user.Email),
            //new Claim(ClaimTypes.Role, user.Role)
          ]),
        Expires = DateTime.UtcNow.AddHours(hoursValidity),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
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
