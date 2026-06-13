using InitiativeTracker.Application.Dtos;
using InitiativeTracker.Domain.Entities;
using InitiativeTracker.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace InitiativeTracker.Application;

public interface IItemService
{
    Task AddAsync(ItemCreateDto dto);
    Task UpdateAsync(int id, ItemUpdateDto dto);
    Task DeleteAsync(int id);
    Task<ItemEntity?> GetByIdAsync(int id);
    Task<IReadOnlyList<ItemEntity>> SearchAsync(string query);
}

public class ItemService(
    ILogger<ItemService> logger,
    InitiativeTrackerDbContext dbContext
) : IItemService
{
    public async Task AddAsync(ItemCreateDto dto)
    {
        try
        {
            var entity = new ItemEntity
            {
                Name = dto.Name,
                Rarity = dto.Rarity,
                RequiresAttunement = dto.RequiresAttunement,
                Description = dto.Description,
                PrintedCount = dto.PrintedCount,
            };

            dbContext.Items.Add(entity);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to add item from DTO.");
            throw;
        }
    }

    public async Task UpdateAsync(int id, ItemUpdateDto dto)
    {
        try
        {
            var entity = await dbContext.Items.FindAsync(id);
            if (entity == null)
                return;

            if (!string.IsNullOrEmpty(dto.Name))
                entity.Name = dto.Name;
            if (dto.Rarity.HasValue)
                entity.Rarity = dto.Rarity.Value;
            if (dto.RequiresAttunement.HasValue)
                entity.RequiresAttunement = dto.RequiresAttunement.Value;
            if (dto.Description != null)
                entity.Description = dto.Description;
            if (dto.PrintedCount.HasValue)
                entity.PrintedCount = dto.PrintedCount.Value;

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
            var entity = await dbContext.Items.FindAsync(id);
            if (entity == null)
                return;

            dbContext.Items.Remove(entity);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to delete item with id {Id}.", id);
            throw;
        }
    }

    public Task<ItemEntity?> GetByIdAsync(int id)
        => dbContext.Items.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);

    public async Task<IReadOnlyList<ItemEntity>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return await dbContext.Items.AsNoTracking().ToListAsync();

        return await dbContext.Items
            .AsNoTracking()
            .Where(e => EF.Functions.Like(e.Name, $"%{query}%"))
            .ToListAsync();
    }
}
