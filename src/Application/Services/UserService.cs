using Users.src.Application.Dtos;
using Users.src.Domain.Contracts;
using Users.src.Domain.Core;
using Users.src.Domain.Entities;

namespace Users.src.Application.Services
{
  public class UserService : IUserService
  {
    readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
      _repository = repository;
    }

    public async Task<UserDto?> Find(int id)
    {
      var row = await _repository.Find(id);
      if (row == null)
        return null;
      return UserToDto(row);
    }
    
    public async Task<User?> FindByEmail(string email)
    {
      var row = await _repository.FindByEmail(email);
      return row;
    }

    public async Task<Pagination<UserDto>> FindAll(int page, int perPage)
    {
      //pagination
      page = Math.Max(page, 1);
      perPage = Math.Min(perPage, 50);

      //sort & order  
      const string sort = "Name";
      const string order = "asc";

      var users = await _repository.FindAll(page, perPage, sort, order);
      var result = UsersToDto(users);
      return result;
    }

    public async Task<UserDto> Save(User user)
    {
      if (string.IsNullOrWhiteSpace(user.Name))
        user.Name = user.Email;

      user.Password = HashPassword(user.Password);

      var dto = UserToDto(await _repository.Save(user));
      return dto;
    }

    public async Task<bool> Delete(int id)
    {
      var user = await _repository.Find(id);
      if (user == null)
        return false;

      return await _repository.Delete(user);
    }

    public async Task<bool> UnDelete(int id)
    {
      var user = await _repository.Find(id, false);
      if (user == null)
        return false;

      return await _repository.UnDelete(user);
    }

    private Pagination<UserDto> UsersToDto(Pagination<User> users)
    {
      var result = new Pagination<UserDto>()
      {
        Page = users.Page,
        PerPage = users.PerPage,
        PageCount = users.PageCount,
        RecordCount = users.RecordCount,
        Data = users.Data?.Select(x => UserToDto(x)).ToList(),
      };
      return result;
    }

    private UserDto UserToDto(User user)
    {
      var dto = new UserDto()
      {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email,
        IsAdmin = user.IsAdmin,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt,
        DeletedAt = user.DeletedAt,
      };
      return dto;
    }

    private static string HashPassword(string pass)
    {
      return BCrypt.Net.BCrypt.HashPassword(pass);
    }
         
  }
}
