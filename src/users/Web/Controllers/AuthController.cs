using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Users.src.Domain.Contracts;
using Users.src.Domain.Entities;

namespace Users.src.Web.Controllers
{
  [Route("api/[controller]")]
  public class AuthController : Controller
  {
    readonly IAuthService _service;

    public AuthController(IAuthService auth)
    {
      _service = auth;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult> Login([FromBody] User? user)
    {
      if (user== null || string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.Password))
        return BadRequest();

#if DEBUG
      //FIXME: temp
      if (user.Email == "ms")
        user.Email = "ms@mail.com";
      if (user.Email == "jd")
        user.Email = "jd@mail.com";
#endif

      (string token, user) = await _service.Login(user.Email, user.Password);
      if (token == "" || user == null)
        return Unauthorized();

      return Ok(new
      {
        token = token,
        user = new {
          id = user.Id,
          email = user.Email,
          name = user.Name,
          isAdmin = user.IsAdmin,
        }
      });
    }

  }
}
