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

            return services;
        }

        public IServiceCollection AddDatabase(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Default");
            services.AddSingleton(sp =>
            {
                var options = new DbContextOptionsBuilder<InitiativeTrackerDbContext>()
                    .UseSqlite(connectionString)
                    .Options;
                return new InitiativeTrackerDbContext(options);
            });
            return services;
        }

        public IServiceCollection AddApplication()
        {
            services.AddSingleton<IInitiativeService, InitiativeService>();
            services.AddScoped<IMiniatureService, MiniatureService>();
            services.AddScoped<IItemService, ItemService>();
            services.AddScoped<ISpellService, SpellService>();

            return services;
        }
    }
}