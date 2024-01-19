using ExcalibConsultBot.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace ExcalibConsultBot.DAL;

public class ConsultDbContext : DbContext
{
    public ConsultDbContext(DbContextOptions<ConsultDbContext> options) : base(options)
    {
    }
    
    public DbSet<MessageEntity> Messages { get; set; } = null!;
    public DbSet<UserEntity> Users { get; set; } = null!;
}