using InitiativeTracker.DataAccess.Dtos;
using InitiativeTracker.Domain.Entities;
using InitiativeTracker.Infrastructure;
using InitiativeTracker.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace InitiativeTracker.DataAccess.Repositories;

public interface IEncounterRepository : IRepository, IWarmUp
{
    Task<List<Encounter>> GetAllEncountersAsync();
    Task<Encounter?> GetEncounterByIdAsync(int id);
    Task<Encounter> CreateEncounterAsync(EncounterCreateDto dto);
    Task<Encounter?> CopyEncounterAsync(int sourceEncounterId, string name);
    Task UpdateEncounterAsync(EncounterUpdateDto dto);
    Task DeleteEncounterAsync(int id);
}

public class EncounterRepository(
    ILogger<EncounterRepository> logger,
    InitiativeTrackerDbContext dbContext
) : IEncounterRepository
{
    public async Task WarmUpAsync()
    {
        try
        {
            var encounters = await GetAllEncountersAsync();

            if (!encounters.Any())
            {
                await CreateEncounterAsync(new EncounterCreateDto("Default"));
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to load encounters from database.");
        }
    }

    public async Task<List<Encounter>> GetAllEncountersAsync()
    {
        return await dbContext.Encounters
            .AsNoTracking()
            .OrderBy(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Encounter?> GetEncounterByIdAsync(int id)
    {
        return await dbContext.Encounters.FindAsync(id);
    }

    public async Task<Encounter> CreateEncounterAsync(EncounterCreateDto dto)
    {
        try
        {
            var encounter = new Encounter
            {
                Name = dto.Name,
                CreatedAt = DateTime.UtcNow,
                CurrentRound = 1,
            };

            dbContext.Encounters.Add(encounter);
            await dbContext.SaveChangesAsync();
            return encounter;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to create encounter");
            throw;
        }
    }

    public async Task<Encounter?> CopyEncounterAsync(int sourceEncounterId, string name)
    {
        try
        {
            var source = await dbContext.Encounters
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == sourceEncounterId);
            if (source is null)
                return null;

            var sourceParticipants = await dbContext.EncounterParticipants
                .AsNoTracking()
                .Where(p => p.EncounterId == sourceEncounterId)
                .OrderBy(p => p.Order)
                .ToListAsync();

            var copy = new Encounter
            {
                Name = name,
                CreatedAt = DateTime.UtcNow,
                CurrentRound = 1,
                SkipDead = source.SkipDead,
            };

            foreach (var participant in sourceParticipants)
            {
                var clone = participant.CopyAsTemplate();
                clone.Encounter = copy;
                dbContext.EncounterParticipants.Add(clone);
            }

            dbContext.Encounters.Add(copy);
            await dbContext.SaveChangesAsync();
            return copy;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to copy encounter with Id {Id}.", sourceEncounterId);
            throw;
        }
    }

    public async Task UpdateEncounterAsync(EncounterUpdateDto dto)
    {
        try
        {
            var entity = await dbContext.Encounters.FindAsync(dto.Id);
            if (entity is null)
                return;
            
            if (!string.IsNullOrEmpty(dto.Name))
                entity.Name = dto.Name;
            if (dto.CurrentRound is not null)
                entity.CurrentRound = dto.CurrentRound.Value;
            if (dto.CurrentActiveParticipantId is not null)
                entity.CurrentActiveParticipantId = dto.CurrentActiveParticipantId.Value;
            if (dto.SkipDead is not null)
                entity.SkipDead = dto.SkipDead.Value;

            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to update encounter \"{Name}\".", dto.Name);
            throw;
        }
    }

    public async Task DeleteEncounterAsync(int id)
    {
        try
        {
            await dbContext.Encounters
                .Where(p => p.Id == id)
                .ExecuteDeleteAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to delete encounter with Id {Id}.", id);
            throw;
        }
    }
}
