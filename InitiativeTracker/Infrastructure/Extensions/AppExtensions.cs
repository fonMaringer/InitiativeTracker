using InitiativeTracker.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace InitiativeTracker.Infrastructure.Extensions;

public static class AppExtensions
{
    extension(WebApplication app)
    {
        public async Task WarmUp()
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<InitiativeTrackerDbContext>();
            await dbContext.Database.MigrateAsync();

            var initiativeService = scope.ServiceProvider.GetServices<IWarmUp>();
            foreach (var service in initiativeService)
            {
                await service.WarmUpAsync();
            }
        }

        public void TearDown()
        {
        }
    }
}