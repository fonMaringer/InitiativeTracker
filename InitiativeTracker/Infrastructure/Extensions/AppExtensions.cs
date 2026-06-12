using InitiativeTracker.Application;
using InitiativeTracker.Infrastructure.Database;

namespace InitiativeTracker.Infrastructure.Extensions;

public static class AppExtensions
{
    public static void WarmUp(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InitiativeTrackerDbContext>();
        dbContext.Database.EnsureCreated();

        var initiativeService = scope.ServiceProvider.GetRequiredService<IInitiativeService>();
        initiativeService.WarmUp();
    }

    public static void TearDown(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var initiativeService = scope.ServiceProvider.GetRequiredService<IInitiativeService>();
        initiativeService.SaveToFile();
    }
}