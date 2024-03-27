namespace ExcalibConsultBot.DAL.Models;

public class LessonEntity : BaseEntity
{
    public DateTime? LessonDate { get; set; }
    public bool IsFinished { get; set; }
    public long? UserId { get; set; }
    public UserEntity? User { get; set; }
    public int LessonCount { get; set; } = 1;
}