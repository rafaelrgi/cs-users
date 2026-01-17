using System.ComponentModel.DataAnnotations;
using Users.src.Application.Dtos;
using Users.src.Domain.Common;
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
      //perPage = Math.Min(perPage, 50);      

      //sort & order  
      const string sort = "Name";
      const string order = "asc";

      var users = await _repository.FindAll(page, perPage, sort, order);
      var result = UsersToDto(users);
      return result;
    }

    public async Task<Result<UserDto>> Save(UserSaveDto dto, int? userId = null)
    {
      int id = userId ?? 0;
      User? row = (id > 0) ? await _repository.Find(id) : new User();
      UserFromDto(row, dto);

      row!.Id = id;
      if (string.IsNullOrWhiteSpace(row!.Name))
        row.Name = row.Email;

      //save password?
      if (!string.IsNullOrWhiteSpace(dto.PasswordNew))
      {
        //changing password, check the old one
        if (row.Id > 0 && !AuthService.CheckPassword(dto.Password, row.Password))
          return new(null, false, true);

        if (dto.PasswordNew != dto.PasswordCheck)
          return new(null, false, false, "Passwords do not match");

        row.Password = HashPassword(dto.PasswordNew);
      }

      //activating/deactivating the user?
      if (row.IsDeleted != dto.IsDeleted)
      {
        if (dto.IsDeleted)
          row.DeletedAt = DateTime.Now;
        else
          row.DeletedAt = null;
      }

      //UNDONE: base service maybe? base entity maybe?
      var results = new List<ValidationResult>();
      var context = new ValidationContext(row, serviceProvider: null, items: null);
      if (!Validator.TryValidateObject(row, context, results, validateAllProperties: true))
      {
        string s = string.Join(" \r\n", results);
        if (string.IsNullOrWhiteSpace(s))
          s = "The object is invalid.";
        return new(null, false, false, s);
      }

      var result = UserToDto(await _repository.Save(row));
      return new(result, true);
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
      var dto = new UserDto(
        user.Id,
        user.Name,
        user.Email,
        user.IsAdmin,
        (user.DeletedAt != null),
        user.CreatedAt,
        user.UpdatedAt,
        user.DeletedAt
      );
      return dto;
    }

    private User UserFromDto(UserSaveDto dto)
    {
      var user = new User();
      UserFromDto(user, dto);
      return user;
    }
    private void UserFromDto(User? user, UserSaveDto dto)
    {
      if (user == null)
        throw new NullReferenceException();
      user.Name = dto.Name;
      user.Email = dto.Email;
      user.IsAdmin = dto.IsAdmin;
    }

    private static string HashPassword(string pass)
    {
      return BCrypt.Net.BCrypt.HashPassword(pass);
    }

  }
}
