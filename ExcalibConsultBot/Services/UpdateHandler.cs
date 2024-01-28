using ExcalibConsultBot.DAL;
using ExcalibConsultBot.DAL.Models;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using MessageEntity = ExcalibConsultBot.DAL.Models.MessageEntity;

namespace ExcalibConsultBot.Services;

public class UpdateHandler : IUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<UpdateHandler> _logger;
    private readonly CurrentState _state;
    private readonly long _adminUserId;
    private readonly ConsultDbContext _context;

    public UpdateHandler(ITelegramBotClient botClient, ILogger<UpdateHandler> logger, CurrentState state, IConfiguration configuration, ConsultDbContext context)
    {
        _botClient = botClient;
        _logger = logger;
        _state = state;
        _context = context;
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
        const string rules = "⏰ Длительность \n \n" +
                             "Стандартное занятие длится 60 минут. Если требуется больше времени, то вы можете договориться о продлении. Превышение времени оплачивается отдельно после проведённого занятия. Если занятие заняло меньше оплаченного времени, то оставшаяся сумма переносится на будущее занятие (то есть при покупке следующего занятия можно позаниматься дольше). \n \n" +
                             "💰 Оплата \n \n" +
                             "На данный момент оплата производится на карту российского банка. При необходимости оплаты по договору(для юр. лиц) нужно согласовать заранее. Оплачивать необходимо до занятия 100% от суммы. Оплату необходимо произвести не позднее чем за 4 часа до назначенного занятия. Если оплата не была осуществлена, то занятие будет отменено. Занятие может быть забронировано менее чем за 4 часа до начала, в таком случае оплата должна быть произведена сразу после бронирования. \n \n" +
                             "💻 Занятие \n \n" +
                             "Ссылку на занятие я присылаю в личные сообщения до начала занятия. Занятие будет записано и видео направлено в личные сообщения. Если вы желаете вести запись занятия на своей стороне или вообще не записывать занятие, то сообщите об этом в чате. \n \n" +
                             "❌ Отмена или перенос \n \n" +
                             "Вы можете перенести или отменить занятие не позднее чем за 4 часа до начала. В случае более поздней отмены или неявки: \n" +
                             "- ученика - возвращается сумма в размере 50% от оплаченной за занятие суммы; \n" +
                             "- меня - ученик получает скидку 50% на следующее занятие или дополнительные 30 минут.\n" +
                             "Если занятие забронировано меньше, чем за 4 часа, то при отмене и переносе занятия действует штраф (прописанный выше). \n \n" +
                             "🏃‍♀️Опоздание \n \n" +
                             "В случае опоздания (15 и более минут) без предупреждения: \n" +
                             "- ученика - занятие продолжается в оставшееся оплаченное время, оплата при этом списывается полностью за всё занятие; \n" +
                             "- меня - ученик получает скидку 50% на следующее занятие или дополнительные 30 минут. \n \n" +
                             "Данные условия могут быть пересмотрены при индивидуальной договорённости.";
        const string prices = "Так как я только начал предоставлять услугу консультаций, на первое время я решил сделать скидку на занятия! А именно 25% скидки! Действуют пакетные предложения! Вы можете купить 1, 5 или 10 занятий. \n \n" + 
                              "Цены указаны за час консультации: \n \n" +
                              "1 занятие - 3 000 рублей(без скидки 4000 рублей) \n" + 
                              "5 занятий - 2 850 рублей(без скидки 3750 рублей) \n" +
                              "10 занятий - 2.65 рублей(без скидки 3550 рублей)";
        
        _logger.LogInformation("Receive message type: {MessageType}", message.Type);
        var userId = message.From?.Id ?? message.Chat.Id;
        var user = _context.Users.FirstOrDefault(x => x.UserId == userId);
        
        if (user == null)
        {
            user = new UserEntity
            {
                Name = message.From != null ? $"{message.From?.FirstName} {message.From?.LastName}" : $"{message.Chat.FirstName} {message.Chat.LastName}",
                Username = message.From?.Username ?? message.Chat.Username,
                UserId = message.From?.Id
            };

            var userEntity = await _context.Users.AddAsync(user, cancellationToken);
            
            await _context.SaveChangesAsync(cancellationToken);
            user.Id = userEntity.Entity.Id;
        }
        
        await _context.Messages.AddAsync(new MessageEntity
        {
            Text = message.Text,
            Type = message.Type,
            MessageId = message.MessageId,
            UserId = user.Id
        }, cancellationToken);
        
        await _context.SaveChangesAsync(cancellationToken);
        
        if (message.Text is not { } messageText)
            return;

        if (message.Text != "/start")
        {
            if (_state.LastMessages.ContainsKey(message.Chat.Id) && _state.LastMessages[message.Chat.Id] == "/start")
            {
                await _botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Спасибо! Я скоро тебе напишу 😽 Для ознакомления с правилами проведения занятий напишите /rules",
                    replyMarkup: new ReplyKeyboardRemove(),
                    cancellationToken: cancellationToken);
            }
            
            if (message.Text == "/rules")
            {
                await _botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: rules, cancellationToken: cancellationToken);
            }
            else if (message.Text == "/prices")
            {
                await _botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: prices, cancellationToken: cancellationToken);
            }
            else
            {
                await _botClient.ForwardMessageAsync(_adminUserId, message.Chat.Id, message.MessageId, cancellationToken: cancellationToken);
            }
            
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