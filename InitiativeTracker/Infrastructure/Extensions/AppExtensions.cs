using InitiativeTracker.Application;
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
            await dbContext.Database.EnsureCreatedAsync();
            //TODO

            var initiativeService = scope.ServiceProvider.GetRequiredService<IInitiativeService>();
            await initiativeService.WarmUpAsync();
        }

        public void TearDown()
        {
            using var scope = app.Services.CreateScope();
            var initiativeService = scope.ServiceProvider.GetRequiredService<IInitiativeService>();
            _ = initiativeService.SaveAllAsync();
        }
    }
}