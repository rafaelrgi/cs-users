using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using Users.Data;
using Users.src.Application.Services;
using Users.src.Domain.Contracts;
using Users.src.Infra.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<Db>(options =>
{
  options.UseMySQL(builder.Configuration.GetConnectionString(Db.ConnectionName)!);
});
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Configure the HTTP request pipeline.
var publicKey = builder.Configuration["RsaKeys:PublicKey"] ?? "";
var rsa = RSA.Create();
rsa.ImportFromPem(publicKey.ToCharArray());

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
  //bearer.SaveToken = true;
  options.RequireHttpsMetadata = false;
  options.Authority = JwtBearerDefaults.AuthenticationScheme;
  options.Audience = JwtBearerDefaults.AuthenticationScheme;
  options.TokenValidationParameters = new TokenValidationParameters
  {
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new RsaSecurityKey(rsa),
    ValidateIssuer = false,
    ValidateAudience = false,
    ValidateLifetime = true,
  };
});

//Require authorization by default for all requests
builder.Services.AddAuthorization(options =>
{
  options.FallbackPolicy = new AuthorizationPolicyBuilder()
      .RequireAuthenticatedUser()
      .Build();
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
