using InitiativeTracker.Application;
using InitiativeTracker.Infrastructure.Database;
using InitiativeTracker.Integration.RestClients.TtgClub;
using Microsoft.EntityFrameworkCore;

namespace InitiativeTracker.Infrastructure.Extensions;

public static class DiExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddHttpClients(IConfiguration configuration)
        {
            services.Configure<TtgClubClientOptions>(configuration.GetSection(nameof(TtgClubClientOptions)));
            services.AddSingleton<IBestiaryClient, BestiaryClient>();
            services.AddSingleton<IMagicItemsClient, MagicItemsClient>();
            services.AddSingleton<ISpellsClient, SpellsClient>();

            return services;
        }

        public IServiceCollection AddDatabase(IConfiguration configuration)
        {
            var rawConnectionString = configuration.GetConnectionString("Default");
            var dbFileName = rawConnectionString!.Replace("Data Source=", "", StringComparison.OrdinalIgnoreCase);
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dbFileName);
            var connectionString = $"Data Source={dbPath}";
            
            services.AddDbContext<InitiativeTrackerDbContext>(options =>
                options.UseSqlite(connectionString), ServiceLifetime.Singleton);
            return services;
        }

        public IServiceCollection AddApplication()
        {
            services.AddSingleton<IInitiativeService, InitiativeService>();
            services.AddSingleton<IParticipantLibraryService, ParticipantLibraryService>();
            services.AddSingleton<IMiniatureService, MiniatureService>();
            services.AddSingleton<IItemService, ItemService>();
            services.AddSingleton<ISpellService, SpellService>();

            return services;
        }
    }
}