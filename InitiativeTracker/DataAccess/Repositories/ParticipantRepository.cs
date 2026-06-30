using InitiativeTracker.DataAccess.Dtos;
using InitiativeTracker.Domain.Entities;
using InitiativeTracker.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace InitiativeTracker.DataAccess.Repositories;

public interface IParticipantRepository : IRepository
{
    Task<IReadOnlyCollection<ParticipantCatalogItem>> GetAllAsync();

    Task<ParticipantCatalogItem?> GetByIdAsync(int id);

    Task<int> CreateAsync(ParticipantCreateDto dto);

    Task UpdateAsync(ParticipantUpdateDto dto);

    Task DeleteAsync(int id);
}

public class ParticipantRepository(
    ILogger<ParticipantRepository> logger,
    Infrastructure.Database.InitiativeTrackerDbContext dbContext
) : IParticipantRepository
{
    public async Task<IReadOnlyCollection<ParticipantCatalogItem>> GetAllAsync()
    {
        return await dbContext.ParticipantCatalog
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<ParticipantCatalogItem?> GetByIdAsync(int id)
    {
        return await dbContext.ParticipantCatalog.FindAsync(id);
    }

    public async Task<int> CreateAsync(ParticipantCreateDto dto)
    {
        try
        {
            var entity = new ParticipantCatalogItem
            {
                Name = dto.Name,
                Dexterity = dto.Dexterity,
                Hits = dto.Hp,
                ArmorClass = dto.Ac
            };

            dbContext.ParticipantCatalog.Add(entity);
            await dbContext.SaveChangesAsync();
            return entity.Id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to create participant \"{Name}\".", dto.Name);
            throw;
        }
    }

    public async Task UpdateAsync(ParticipantUpdateDto dto)
    {
        try
        {
            var entity = await dbContext.ParticipantCatalog.FindAsync(dto.Id);
            if (entity is null)
                return;
            
            entity.Name = dto.Name;
            entity.Dexterity = dto.Dexterity;
            entity.Hits = dto.Hp;
            entity.ArmorClass = dto.Ac;

            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to update participant \"{Name}\".", dto.Name);
            throw;
        }
    }

    public async Task DeleteAsync(int id)
    {
        try
        {
            await dbContext.ParticipantCatalog
                .Where(p => p.Id == id)
                .ExecuteDeleteAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to delete participant with Id {Id}.", id);
            throw;
        }
    }
}
