using Users.src.Application.Dtos;
using Users.src.Domain.Core;
using Users.src.Domain.Entities;

namespace Users.src.Domain.Contracts
{
  public interface IUserService
  {
    Task<UserDto?> Find(int id);
    Task<User?> FindByEmail(string email);
    Task<Pagination<UserDto>> FindAll(int page, int perPage);
    Task<UserDto> Save(User user);
    Task<bool> Delete(int id);
    Task<bool> UnDelete(int id);
  }
}
