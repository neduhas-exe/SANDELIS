// Presentation/Controllers/UserController.cs
using Microsoft.AspNetCore.Mvc;
using Domain.Models;


namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id}")]
    public IActionResult Get(long id)
    {
        var user = _userService.GetById(id);
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpGet]
    public IActionResult List()
    {
        return Ok(_userService.List());
    }

    [HttpGet("username/{username}")]
    public IActionResult GetByUsername(string username)
    {
        var user = _userService.GetByUsername(username);
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpPost]
    public IActionResult Create([FromBody] User user)
    {
        if (user == null)
            return BadRequest();

        var createdUser = _userService.Create(user);
        return CreatedAtAction(nameof(Get), new { id = createdUser.Id }, createdUser);
    }

    [HttpPut("{id}")]
    public IActionResult Update(long id, [FromBody] User user)
    {
        if (user == null || id != user.Id)
            return BadRequest();

        var updatedUser = _userService.Update(user);
        if (updatedUser == null)
            return NotFound();

        return Ok(updatedUser);
    }
}
