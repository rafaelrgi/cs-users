using Users.src.Domain.Core;
using Users.src.Domain.Entities;

namespace Users.src.Domain.Contracts
{
  public interface IUserRepository
  {
    Task<User?> Find(int id, bool ignoreDeleted=true);
    Task<Pagination<User>> FindAll(int page, int perPage, string sort, string order);
    Task<User> Save(User user);
    Task<bool> Delete(User user);
    Task<bool> UnDelete(User user);
    Task<User?> FindByEmail(string email);
  }
}
