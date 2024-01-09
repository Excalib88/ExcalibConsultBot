namespace ExcalibConsultBot.Services;

public class CurrentState
{
    public Dictionary<long, string?> LastMessages { get; set; } = new();

    public void AddOrUpdate(long userId, string? text)
    {
        if (LastMessages.ContainsKey(userId))
        {
            LastMessages[userId] = text;
            return;
        }
        
        LastMessages.Add(userId, text);
    }

    public string? GetLastMessage(long userId)
    {
        return LastMessages.ContainsKey(userId) ? LastMessages[userId] : null;
    }
}