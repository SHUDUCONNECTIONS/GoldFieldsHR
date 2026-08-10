using GoldFieldsHR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GoldFieldsHR.Infrastructure.Auth;

public class RefreshTokenCleanupService(
    IServiceScopeFactory scopeFactory, ILogger<RefreshTokenCleanupService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);
        do
        {
            await CleanupExpiredTokensAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task CleanupExpiredTokensAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var expired = await dbContext.RefreshTokens
                .Where(t => t.ExpiresAtUtc < DateTime.UtcNow)
                .ToListAsync(cancellationToken);

            if (expired.Count == 0)
            {
                return;
            }

            dbContext.RefreshTokens.RemoveRange(expired);
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Removed {Count} expired refresh token(s).", expired.Count);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Refresh token cleanup failed.");
        }
    }
}
