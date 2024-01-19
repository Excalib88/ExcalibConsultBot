using Telegram.Bot.Types.Enums;

namespace ExcalibConsultBot.DAL.Models;

public class MessageEntity : BaseEntity
{
    public string? Text { get; set; }
    public MessageType Type { get; set; }
    public long MessageId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public UserEntity? User { get; set; }
    public long? UserId { get; set; }
}