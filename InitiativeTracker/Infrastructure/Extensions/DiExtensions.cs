using InitiativeTracker.Integration.RestClients.TtgClub;

namespace InitiativeTracker.Infrastructure.Extensions;

public static class DiExtensions
{
    public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TtgClubClientOptions>(configuration.GetSection(nameof(TtgClubClientOptions)));
        services.AddSingleton<IBestiaryClient, BestiaryClient>();

        return services;
    }
}