using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Users.src.Web.Controllers
{
  [AllowAnonymous]
  [Route("api/")]
  public class HomeController : Controller
  {
    [HttpGet]
    public ActionResult<string> Index()
    {
      bool isDocker = Environment.GetEnvironmentVariable("IS_DOCKER") == "true";

      bool isAuth = (User.Identity != null && User.Identity.IsAuthenticated);

      var result = new
      {
        Status = "API is ready!",
        IsDocker = isDocker,
        IsAuth = isAuth,
#if DEBUG
        BaseDir = AppContext.BaseDirectory,
#endif
        User = !isAuth ? null : new
        {
          Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
#if DEBUG
          AuthType = User.Identity!.AuthenticationType,
#endif
          Name = User!.Identity!.Name,
          IsAdmin = User.IsInRole("Admin"),
        }
      };

      return Ok(result);
    }

  }
}
