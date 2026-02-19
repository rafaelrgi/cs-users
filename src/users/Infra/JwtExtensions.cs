using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace Users.src.Infra.Extensions;

public static class JwtExtensions
{
  public static IServiceCollection AddRsaJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
  {
    using var loggerFactory = LoggerFactory.Create(logging => logging.AddConsole());
    var startupLogger = loggerFactory.CreateLogger("JwtExtension");

    var publicKeyPath = configuration["RsaKeys:PublicKeyPath"];
    var rsa = RSA.Create();

    if (!string.IsNullOrEmpty(publicKeyPath) && File.Exists(publicKeyPath))
    {
      var pemContent = File.ReadAllText(publicKeyPath);
      rsa.ImportFromPem(pemContent);
    }
    else
    {
      startupLogger.LogWarning("RSA Public Key NOT FOUND at: {Path}", publicKeyPath);
    }

    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
          options.TokenValidationParameters = new TokenValidationParameters
          {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new RsaSecurityKey(rsa),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true
          };
        });

    return services;
  }
}