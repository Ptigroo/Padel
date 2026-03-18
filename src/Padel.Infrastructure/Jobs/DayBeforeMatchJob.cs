using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Padel.Application.Interfaces;

namespace Padel.Infrastructure.Jobs;

public class DayBeforeMatchJob(IServiceScopeFactory scopeFactory, ILogger<DayBeforeMatchJob> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("DayBeforeMatchJob started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var matchService = scope.ServiceProvider.GetRequiredService<IMatchService>();

                logger.LogInformation("Running day-before match processing at {Time}.", DateTime.Now);
                await matchService.ProcessDayBeforeAsync();
                logger.LogInformation("Day-before match processing completed.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during day-before match processing.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }
}
