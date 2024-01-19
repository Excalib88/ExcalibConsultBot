using ExcalibConsultBot.DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExcalibConsultBot.Controllers;

[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    private readonly ConsultDbContext _context;

    public UsersController(ConsultDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.Users.Include(x => x.Messages).ToListAsync();
        
        return Ok(users);
    }

    [HttpGet("messages")]
    public async Task<IActionResult> GetMessages()
    {
        var messages = await _context.Messages.ToListAsync();
        return Ok(messages);
    }
}