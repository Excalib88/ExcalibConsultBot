namespace ExcalibConsultBot.DAL.Models;

public class UserEntity : BaseEntity
{
    public string? Username { get; set; }
    public long? UserId { get; set; }
    public long? ChatId { get; set; }
    public string? Name { get; set; }
    public List<MessageEntity>? Messages { get; set; }
}