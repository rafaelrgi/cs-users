using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Users.src.Application.Dtos;
using Users.src.Domain.Contracts;
using Users.src.Domain.Entities;

namespace Users.src.Web.Controllers
{
  [Route("api/users")]
  public class UserController : Controller
  {
    readonly IUserService _service;
    readonly IAuthService _auth;

    public UserController(IUserService service, IAuthService auth)
    {
      _service = service;
      _auth = auth;
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
      perPage = Math.Min(perPage, 50);

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
    public async Task<ActionResult> Create([FromBody] User user)
    {
      if (!ModelState.IsValid)
        return BadRequest(ModelState.Values.SelectMany(v => v.Errors));

      try
      {
        var dto = await _service.Save(user);
        var uri = new Uri($"api/users/{dto.Id}", UriKind.Relative);
        return Created(uri, dto);
      }
      catch (Exception e)
      {
        Console.WriteLine(e.ToString());
        throw new Exception(e.Message);
      }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Edit(int id, [FromBody] User user)
    {
      if (!ModelState.IsValid)
        return BadRequest(ModelState.Values.SelectMany(v => v.Errors));

      user.Id = id;
      try
      {
        var dto = await _service.Save(user);
        return Ok(dto);
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
