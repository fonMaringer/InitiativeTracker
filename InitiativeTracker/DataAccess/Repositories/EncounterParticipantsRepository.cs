using InitiativeTracker.Domain.Entities;
using InitiativeTracker.Infrastructure;
using InitiativeTracker.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace InitiativeTracker.DataAccess.Repositories;

public interface IEncounterParticipantsRepository : IRepository
{
    Task<List<EncounterParticipant>> GetAllEncounterParticipantsAsync(int encounterId);
    Task SetEncounterParticipantsAsync(int encounterId, List<EncounterParticipant> participants);
}

public class EncounterParticipantsRepository(
    InitiativeTrackerDbContext dbContext
    ) : IEncounterParticipantsRepository
{
    public async Task<List<EncounterParticipant>> GetAllEncounterParticipantsAsync(int encounterId)
    {
        return await dbContext.EncounterParticipants
            .Where(p => p.EncounterId == encounterId)
            .OrderBy(p => p.Order)
            .ToListAsync();
    }

    public async Task SetEncounterParticipantsAsync(int encounterId, List<EncounterParticipant> participants)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        
        var currentParticipants = await GetAllEncounterParticipantsAsync(encounterId);
        dbContext.EncounterParticipants.RemoveRange(currentParticipants);
        dbContext.EncounterParticipants.AttachRange(participants);
        await dbContext.SaveChangesAsync();

        await transaction.CommitAsync();
    }
}