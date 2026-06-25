using InitiativeTracker.Application;
using InitiativeTracker.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace InitiativeTracker.Infrastructure.Extensions;

public static class AppExtensions
{
    extension(WebApplication app)
    {
        public void WarmUp()
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<InitiativeTrackerDbContext>();
            dbContext.Database.EnsureCreated();

            var initiativeService = scope.ServiceProvider.GetRequiredService<IInitiativeService>();
            initiativeService.WarmUp();
        }

        public void TearDown()
        {
            using var scope = app.Services.CreateScope();
            var initiativeService = scope.ServiceProvider.GetRequiredService<IInitiativeService>();
            initiativeService.SaveToFile();
        }
    }
}