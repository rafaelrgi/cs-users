using Users.src.Application.Dtos;
using Users.src.Domain.Common;
using Users.src.Domain.Core;
using Users.src.Domain.Entities;

namespace Users.src.Domain.Contracts
{
  public interface IUserService
  {
    Task<UserDto?> Find(int id);
    Task<User?> FindByEmail(string email);
    Task<Pagination<UserDto>> FindAll(int page, int perPage);
    Task<Result<UserDto>> Save(UserSaveDto dto, int? userId = null);
    Task<bool> Delete(int id);
    Task<bool> UnDelete(int id);
  }
}
