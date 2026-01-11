namespace Users.src.Application.Dtos
{
  /// <summary>
  /// Returns the User to the outside without the Password
  /// </summary>
  public record UserDto
  (
    int Id,
    string Name,
    string Email,
    bool IsAdmin,
    bool IsDeleted,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? DeletedAt
  );

  public record UserSaveDto
  (
    string Name,
    string Email,
    bool IsAdmin,
    bool IsDeleted,
    string Password,
    string PasswordNew,
    string PasswordCheck
  );
}
