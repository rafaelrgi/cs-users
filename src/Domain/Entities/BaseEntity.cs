using System.ComponentModel.DataAnnotations;

namespace Users.src.Domain.Entities
{
  public class BaseEntity
  {
    public bool IsDeleted { get => DeletedAt != null; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    /// <summary> Validate the entity using Data Anotations </summary>
    /// <returns> An empty string if no error found, or the list of errors found </returns>    
    public static string Validate(object entity)
    {
      var results = new List<ValidationResult>();
      var context = new ValidationContext(entity, serviceProvider: null, items: null);
      if (Validator.TryValidateObject(entity, context, results, validateAllProperties: true))
        return "";

      string s = string.Join(" \r\n", results);
      if (string.IsNullOrWhiteSpace(s))
        s = "The object is invalid.";
      return s;
    }
  }
}
