using ExcalibConsultBot.DAL;
using Microsoft.EntityFrameworkCore;

namespace ExcalibConsultBot.Services;

public class LessonsBackgroundWorker : BackgroundService
{
    private readonly IServiceProvider _sp;
    
    public LessonsBackgroundWorker(IServiceProvider sp)
    {
        _sp = sp;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _sp.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ConsultPostgresDbContext>();
            var delay = 60 * 1000;
                
            try
            {
                var lessons = await context.Lessons
                    .Where(x => !x.IsFinished && x.LessonDate < DateTime.Now)
                    .ToListAsync(cancellationToken: stoppingToken);

                foreach (var lesson in lessons)
                {
                    lesson.IsFinished = true;
                }

                await context.SaveChangesAsync(stoppingToken);
                
                await Task.Delay(delay, stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lessons error " +
                                $"Message: {ex.Message}, AdditionalInfo: {ex.InnerException}, StackTrace: {ex.StackTrace}");

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
        // ReSharper disable once FunctionNeverReturns
    }
}