using ExcalibConsultBot.DAL;
using ExcalibConsultBot.Services;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
);;
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient("telegram_bot_client")
    .AddTypedClient<ITelegramBotClient>((httpClient, _) =>
    {
        var token = builder.Configuration["BotConfiguration:BotToken"];
        TelegramBotClientOptions options = new(token!);
        return new TelegramBotClient(options, httpClient);
    });

builder.Services.AddScoped<UpdateHandler>();
builder.Services.AddScoped<ReceiverService>();
builder.Services.AddSingleton<CurrentState>();
builder.Services.AddHostedService<PollingService>();
builder.Services.AddDbContext<ConsultDbContext>(o => 
    o.UseSqlite(builder.Configuration.GetConnectionString("Db")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();