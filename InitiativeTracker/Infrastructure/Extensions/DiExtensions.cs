using InitiativeTracker.DataAccess.Repositories;
using InitiativeTracker.Infrastructure.Database;
using InitiativeTracker.Integration.RestClients.TtgClub;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace InitiativeTracker.Infrastructure.Extensions;

public static class DiExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddHttpClients(IConfiguration configuration)
        {
            services.Configure<TtgClubClientOptions>(configuration.GetSection(nameof(TtgClubClientOptions)));
            services.TryAddScoped<IBestiaryClient, BestiaryClient>();
            services.TryAddScoped<IMagicItemsClient, MagicItemsClient>();
            services.TryAddScoped<ISpellsClient, SpellsClient>();

            return services;
        }

        public IServiceCollection AddDatabase(IConfiguration configuration)
        {
            var rawConnectionString = configuration.GetConnectionString("Default");
            var dbFileName = rawConnectionString!.Replace("Data Source=", "", StringComparison.OrdinalIgnoreCase);
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dbFileName);
            var connectionString = $"Data Source={dbPath}";
            
            services.AddDbContext<InitiativeTrackerDbContext>(options =>
                options.UseSqlite(connectionString));
            return services;
        }

        public IServiceCollection AddApplication()
        {
            services.TryAddScoped<IEncounterRepository, EncounterRepository>();
            services.TryAddScoped<IParticipantRepository, ParticipantRepository>();
            services.TryAddScoped<IMiniatureRepository, MiniatureRepository>();
            services.TryAddScoped<IMagicItemRepository, MagicMagicItemRepository>();
            services.TryAddScoped<ISpellRepository, SpellRepository>();
            services.TryAddScoped<IEncounterParticipantsRepository, EncounterParticipantsRepository>();

            return services;
        }
    }
}