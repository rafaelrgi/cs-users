using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using System.Security.Cryptography.X509Certificates;
using System.Text;
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
    public async Task<ActionResult> Login([FromBody] User user)
    {
      //Console.WriteLine($">>>>>>>>>>User: {user}");
      if (string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.Password))
        return BadRequest();

      var token = await _service.Login(user.Email, user.Password);
      if (token == "")
        return Unauthorized();

      return Ok(new { token = token });
    }

  }
}
