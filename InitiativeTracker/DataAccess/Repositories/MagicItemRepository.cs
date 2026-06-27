using InitiativeTracker.DataAccess.Dtos;
using InitiativeTracker.Domain.Entities;
using InitiativeTracker.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace InitiativeTracker.DataAccess.Repositories;

public interface IMagicItemRepository
{
    Task AddAsync(MagicItemCreateDto dto);
    Task UpdateAsync(int id, MagicItemUpdateDto dto);
    Task DeleteAsync(int id);
    Task<MagicItem?> GetByIdAsync(int id);
    Task<IReadOnlyList<MagicItem>> SearchAsync(string query);
}

public class MagicMagicItemRepository(
    ILogger<MagicMagicItemRepository> logger,
    InitiativeTrackerDbContext dbContext
) : IMagicItemRepository
{
    public async Task AddAsync(MagicItemCreateDto dto)
    {
        try
        {
            var entity = new MagicItem
            {
                Name = dto.Name,
                Type = dto.Type,
                Rarity = dto.Rarity,
                RequiresAttunement = dto.RequiresAttunement,
                Description = dto.Description,
                PrintedCount = dto.PrintedCount,
                Link = dto.Link,
                Source = dto.Source,
            };

            dbContext.MagicItems.Add(entity);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to add item from DTO.");
            throw;
        }
    }

    public async Task UpdateAsync(int id, MagicItemUpdateDto dto)
    {
        try
        {
            var entity = await dbContext.MagicItems.FindAsync(id);
            if (entity == null)
                return;

            if (!string.IsNullOrEmpty(dto.Name))
                entity.Name = dto.Name;
            if (!string.IsNullOrEmpty(dto.Type))
                entity.Type = dto.Type;
            if (dto.Rarity.HasValue)
                entity.Rarity = dto.Rarity.Value;
            if (dto.RequiresAttunement.HasValue)
                entity.RequiresAttunement = dto.RequiresAttunement.Value;
            if (dto.Description != null)
                entity.Description = dto.Description;
            if (dto.PrintedCount.HasValue)
                entity.PrintedCount = dto.PrintedCount.Value;
            entity.Link = dto.Link;

            await dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to update item with id {Id}.", id);
            throw;
        }
    }

    public async Task DeleteAsync(int id)
    {
        try
        {
            var entity = await dbContext.MagicItems.FindAsync(id);
            if (entity == null)
                return;

            dbContext.MagicItems.Remove(entity);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to delete item with id {Id}.", id);
            throw;
        }
    }

    public Task<MagicItem?> GetByIdAsync(int id)
        => dbContext.MagicItems.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);

    public async Task<IReadOnlyList<MagicItem>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return await dbContext.MagicItems.AsNoTracking().ToListAsync();

        return await dbContext.MagicItems
            .AsNoTracking()
            .Where(e => EF.Functions.Like(e.Name, $"%{query}%"))
            .ToListAsync();
    }
}
