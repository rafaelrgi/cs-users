using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using Users.src.Application.Services;
using Users.src.Domain.Contracts;
using Users.src.Infra.Repositories;
using Users.Infra;
using Users.src.Infra.Extensions;

var builder = WebApplication.CreateBuilder(args);

using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<Db>(options =>
{
  options.UseNpgsql(builder.Configuration.GetConnectionString(Db.ConnectionName)!,
  x => x.MigrationsAssembly("Users"));
});
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Authorization
builder.Services.AddRsaJwtAuthentication(builder.Configuration);

//Require authorization by default for all requests
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

//Cors configuration
builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowAngularApp",
      builder =>
      {
        builder.WithOrigins("http://localhost:4200")
                 .AllowAnyHeader()
                 .AllowAnyMethod();
      });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors("AllowAngularApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
