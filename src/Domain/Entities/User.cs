using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Users.src.Domain.Entities
{
  [Index(nameof(Email))]
  public class User : BaseEntity
  {
    public int Id { get; set; }

    [JsonPropertyName("name")]
    [StringLength(64)]
    public string Name { get; set; } = "";

    [JsonPropertyName("email")]
    [Required, EmailAddress, StringLength(128)]
    public string Email { get; set; } = "";

    [JsonPropertyName("password")]
    [Required, StringLength(128), MinLength(3)]
    public string Password { get; set; } = "";

  }
}
