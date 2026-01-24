using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Users.src.Application.Dtos;
using Users.src.Application.Services;
using Users.src.Domain.Contracts;

namespace Users.src.Web.Controllers
{
  [Route(ROUTE)]
  public class UserController : Controller
  {
    const string ROUTE = "api/users";

    readonly IUserService _service;
    readonly IAuthService _auth;
    readonly ILogger<AuthService> _logger;

    public UserController(IUserService service, IAuthService auth, ILogger<AuthService> logger)
    {
      _service = service;
      _auth = auth;
      _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult> Index()
    {
      //pagination
      if (!int.TryParse(HttpContext.Request.Query["page"], out int page))
        page = 1;
      if (!int.TryParse(HttpContext.Request.Query["perPage"], out int perPage))
        perPage = 10;
      page = Math.Max(page, 1);
      perPage = Math.Max(perPage, 2);

      var result = await _service.FindAll(page, perPage);
      if (!result.HasData)
        return NotFound();

      return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> Details(int id)
    {
      UserDto? row = await _service.Find(id);
      if (row == null)
        return NotFound();

      return Ok(row);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    //[AllowAnonymous]
    public async Task<ActionResult> Create([FromBody] UserSaveDto dto)
    {
      try
      {
        var result = await _service.Save(dto);
        if (result.Forbidden)
          return Forbid();

        if (!result.Success || result.Data == null)
          return BadRequest(result.ErrorMessage);

        var uri = new Uri($"{ROUTE}/{result.Data!.Id}", UriKind.Relative);
        return Created(uri, result.Data);
      }
      catch (Exception e)
      {
        Console.WriteLine(e.ToString());
        throw new NotImplementedException();
      }
    }

    [HttpPut("{id}")]
    //[Authorize(Roles = "Admin")] User can edit his own profile
    public async Task<ActionResult> Edit(int id, [FromBody] UserSaveDto dto)
    {
      try
      {
        var result = await _service.Save(dto, id);
        if (result.Success)
          return Ok(result.Data);

        if (result.Forbidden)
          return Forbid();

        return BadRequest(result.ErrorMessage);
      }
      catch (Exception e)
      {
        Console.WriteLine(e.ToString());
        throw new NotImplementedException();
      }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(int id)
    {
      if (!await _service.Delete(id))
        return NotFound();
      return NoContent();
    }

    [HttpPatch("activate/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> UnDelete(int id)
    {
      if (!await _service.UnDelete(id))
        return NotFound();
      return NoContent();
    }

  }
}
