using InitiativeTracker.Application;

namespace InitiativeTracker.Infrastructure.Extensions;

public static class AppExtensions
{
    public static void WarmUp(this WebApplication app)
    {
        var initiativeService = app.Services.GetRequiredService<IInitiativeService>();
        initiativeService.WarmUp();
    }

    public static void TearDown(this WebApplication app)
    {
        var initiativeService = app.Services.GetRequiredService<IInitiativeService>();
        initiativeService.SaveToFile();
    }
}