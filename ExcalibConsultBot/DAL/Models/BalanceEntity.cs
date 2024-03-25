namespace ExcalibConsultBot.DAL.Models;

public class BalanceEntity : BaseEntity
{
    public long? UserId { get; set; }
    public UserEntity? User { get; set; }
    public long BalanceLessonCount { get; set; }
}