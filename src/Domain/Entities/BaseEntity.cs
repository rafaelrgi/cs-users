namespace Users.src.Domain.Entities
{
  public class BaseEntity
  {
    public bool IsDeleted { get => DeletedAt != null; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
  }
}
