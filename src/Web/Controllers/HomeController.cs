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
        User = !isAuth ? null : new
        {
          Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
          Name = User.Identity!.Name,
          Admin = User.IsInRole("Admin"),
          AuthType = User.Identity.AuthenticationType,
    }
      };

      return Ok(result);
    }

  }
}
