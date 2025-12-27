using System.ComponentModel.DataAnnotations;
using Users.src.Domain.Entities;

namespace Users.src.Application.Dtos
{
  /// <summary>
  /// Returns the User to the outside without the Password
  /// </summary>
  public class UserDto
  {
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public bool IsAdmin { get; set; }
    public bool IsDeleted { get => DeletedAt != null; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
  }
}
