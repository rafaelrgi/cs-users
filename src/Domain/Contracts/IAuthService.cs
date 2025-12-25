using Users.src.Application.Dtos;
using Users.src.Domain.Core;
using Users.src.Domain.Entities;

namespace Users.src.Domain.Contracts
{
  public interface IAuthService
  {    
    Task<string> Login(string email, string password);
  }
}
