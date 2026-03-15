using InitiativeTracker.Application;
using InitiativeTracker.Integration.RestClients.TtgClub;

namespace InitiativeTracker.Infrastructure.Extensions;

public static class DiExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddHttpClients(IConfiguration configuration)
        {
            services.Configure<TtgClubClientOptions>(configuration.GetSection(nameof(TtgClubClientOptions)));
            services.AddSingleton<IBestiaryClient, BestiaryClient>();

            return services;
        }

        public IServiceCollection AddApplication()
        {
            services.AddSingleton<IInitiativeService, InitiativeService>();
        
            return services;
        }
    }
}