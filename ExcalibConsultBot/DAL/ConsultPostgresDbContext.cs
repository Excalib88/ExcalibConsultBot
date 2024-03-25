using ExcalibConsultBot.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace ExcalibConsultBot.DAL;

public class ConsultPostgresDbContext: DbContext
{
    public ConsultPostgresDbContext(DbContextOptions<ConsultPostgresDbContext> options) : base(options)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }
    public DbSet<MessageEntity> Messages { get; set; } = null!;
    public DbSet<UserEntity> Users { get; set; } = null!;
    public DbSet<LessonEntity> Lessons { get; set; } = null!;
    public DbSet<BalanceEntity> Balances { get; set; } = null!;
}