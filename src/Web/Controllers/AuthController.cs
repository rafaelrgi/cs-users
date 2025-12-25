using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Users.src.Domain.Contracts;
using Users.src.Domain.Entities;

namespace Users.src.Web.Controllers
{
  [Route("api/[controller]")]
  public class AuthController : Controller
  {
    readonly IUsersService _service;
    readonly IAuthService _auth;

    public AuthController(IUsersService service, IAuthService auth)
    {
      _service = service;
      _auth = auth;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult> Login([FromBody] User user)
    {
      if (string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.Password))
        return BadRequest();

      var token = await _auth.Login(user.Email, user.Password);
      if (token == "")
        return Unauthorized();

      return Ok(new { token = token });
    }
  }
}
