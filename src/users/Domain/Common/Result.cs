namespace Users.src.Domain.Common
{
  public record Result<T>
  (
    T? Data,
    bool Success = true,
    bool Forbidden = false,
    string ErrorMessage = ""
  )
  {
    public Result(T? data) : this(data, true) { }

    public Result(string ErrorMessage, bool Forbidden = false) : this(default, false, Forbidden, ErrorMessage) { }

  }
}
