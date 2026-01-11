namespace Users.src.Domain.Common
{
  public record Result<T>
  (
    T? Data,
    bool Success = true,
    bool Forbidden=false,
    string ErrorMessage = ""
  );
}
