using ExcalibConsultBot.DAL;
using ExcalibConsultBot.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExcalibConsultBot.Controllers;

[ApiController]
[Route("lessons")]
public class LessonsController(ConsultPostgresDbContext context, TokenValidator validator) : ControllerBase
{
    private readonly ConsultPostgresDbContext _context = context;

    [HttpGet("{username}/{token}")]
    public async Task<IActionResult> GetLessonsByUsername(string username, string token)
    {
        if (!validator.Validate(token)) return Forbid();

        var lessons = await context.Lessons
            .Include(x => x.User)
            .Where(x => x.User != null && x.User!.Username == username)
            .ToListAsync();

        return Ok(lessons);
    }
}