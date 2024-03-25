using ExcalibConsultBot.DAL;
using ExcalibConsultBot.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExcalibConsultBot.Controllers;

[ApiController]
[Route("sync")]
public class SyncController(ConsultDbContext sqlite, ConsultPostgresDbContext context, TokenValidator validator) : ControllerBase
{
    [HttpPost("users/{token}")]
    public async Task<IActionResult> SyncUsers(string token)
    {
        if (!validator.Validate(token)) return Forbid();
        
        var users = await sqlite.Users.ToListAsync();

        foreach (var user in users)
        {
            var newUser = await context.Users.FirstOrDefaultAsync(x => x.Id == user.Id);
            
            if (newUser == null)
            {
                user.CreatedAt = DateTime.UtcNow;
                
                await context.Users.AddAsync(user);
            }
        }

        await context.SaveChangesAsync();
        
        return Ok(new {users});
    }

    [HttpPost("messages/{token}")]
    public async Task<IActionResult> SyncMessages(string token)
    {
        if (!validator.Validate(token)) return Forbid();

        var messages = await sqlite.Messages.ToListAsync();
        
        foreach (var message in messages)
        {
            var newMessage = await context.Users.FirstOrDefaultAsync(x => x.Id == message.Id);
            
            if (newMessage == null)
            {
                message.UpdatedAt = DateTime.UtcNow;
                message.CreatedAt = DateTime.UtcNow;
                await context.Messages.AddAsync(message);
            }
        }
        
        await context.SaveChangesAsync();
        
        return Ok(new {messages});
    }
}