using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ExcalibConsultBot.Services;

public class UpdateHandler : IUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<UpdateHandler> _logger;
    private readonly CurrentState _state;
    private readonly long _adminUserId; 

    public UpdateHandler(ITelegramBotClient botClient, ILogger<UpdateHandler> logger, CurrentState state, IConfiguration configuration)
    {
        _botClient = botClient;
        _logger = logger;
        _state = state;
        _adminUserId = configuration.GetSection("BotConfiguration:AdminUserId").Get<long?>() ?? 409698860;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken cancellationToken)
    {
        var handler = update switch
        {
            { Message: { } message } => BotOnMessageReceived(message, cancellationToken),
            _                        => UnknownUpdateHandlerAsync(update)
        };

        await handler;
    }

    private async Task BotOnMessageReceived(Message message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Receive message type: {MessageType}", message.Type);
        if (message.Text is not { } messageText)
            return;

        if (message.Text != "/start")
        {
            if (_state.LastMessages[message.Chat.Id] == "/start")
            {
                await _botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Спасибо! Я скоро тебе напишу 😽",
                    replyMarkup: new ReplyKeyboardRemove(),
                    cancellationToken: cancellationToken);
            }

            await _botClient.ForwardMessageAsync(_adminUserId, message.Chat.Id, message.MessageId, cancellationToken: cancellationToken);

            _state.AddOrUpdate(message.Chat.Id, messageText);
            return;
        }
            
        const string usage = "Кусь 😽 Меня зовут Дамир или Excalib как тебе удобнее. \n" +
                             "Этот бот поможет нам договориться по поводу личной консультации. \n Я могу помочь как с технической стороны: \n" +
                             "- разобраться с проблемой \n" +
                             "- спроектировать и реализовать проект \n " +
                             "- объяснить тему, которую сложно понять простыми словами \n " +
                             "- помогу с выполнением тестового задания \n" +
                             "Так и с нюансами, связанными с IT: \n" +
                             "- подготовлю к собеседованию \n " +
                             "- помогу с резюме и расскажу как лучше аплаится на вакансии, продумаем стратегию \n" +
                             "- выявлю слабые стороны тебя как кандидата  \n" +
                             "- составлю план развития \n" +
                             "- отвечу на любые интересующие вопросы \n \n" +
                             "Если интересно, мы можем сделать тестовое занятие, где мы познакомимся, обсудим твою проблему и договоримся о первом созвоне.   Для того, чтоб я понял смогу ли я тебе помочь, опиши свою проблему! Что ты хочешь получить от нашей консультации?";

        var sentMessage = await _botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: usage,
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken);
        _logger.LogInformation("The message was sent with id: {SentMessageId}", sentMessage.MessageId);
        
        _state.AddOrUpdate(message.Chat.Id, message.Text);
    }

    private Task UnknownUpdateHandlerAsync(Update update)
    {
        _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }

    public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogInformation("HandleError: {ErrorMessage}", errorMessage);

        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }
}