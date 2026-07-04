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
            .AsNoTracking()
            .Where(p => p.EncounterId == encounterId)
            .OrderBy(p => p.Order)
            .ToListAsync();
    }

    public async Task SetEncounterParticipantsAsync(int encounterId, List<EncounterParticipant> participants)
    {
        var existing = await dbContext.EncounterParticipants
            .Where(p => p.EncounterId == encounterId)
            .ToListAsync();

        var incomingIds = participants.Select(p => p.Id).ToList();
        var toRemove = existing.Where(e => !incomingIds.Contains(e.Id));
        dbContext.EncounterParticipants.RemoveRange(toRemove);

        foreach (var participant in participants)
        {
            var localCopy = existing.FirstOrDefault(e => e.Id == participant.Id);
            if (localCopy == null)
            {
                await dbContext.EncounterParticipants.AddAsync(participant);
            }
            else
            {
                dbContext.Entry(localCopy).CurrentValues.SetValues(participant);
            }
        }

        await dbContext.SaveChangesAsync();
    }
}