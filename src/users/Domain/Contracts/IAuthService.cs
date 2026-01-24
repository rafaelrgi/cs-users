using Users.src.Domain.Entities;

namespace Users.src.Domain.Contracts
{
  public interface IAuthService
  {
    Task<(string token, User? user)> Login(string email, string password);
  }
}
