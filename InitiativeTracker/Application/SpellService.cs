using InitiativeTracker.Application.Dtos;
using InitiativeTracker.Domain.Entities;
using InitiativeTracker.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace InitiativeTracker.Application;

public interface ISpellService
{
    Task AddAsync(SpellCreateDto dto);
    Task UpdateAsync(int id, SpellUpdateDto dto);
    Task DeleteAsync(int id);
    Task<SpellEntity?> GetByIdAsync(int id);
    Task<IReadOnlyList<SpellEntity>> SearchAsync(string query);
}

public class SpellService(
    ILogger<SpellService> logger,
    InitiativeTrackerDbContext dbContext
) : ISpellService
{
    public async Task AddAsync(SpellCreateDto dto)
    {
        try
        {
            var entity = new SpellEntity
            {
                Name = dto.Name,
                Type = dto.Type,
                VerbalComponent = dto.VerbalComponent,
                SomaticComponent = dto.SomaticComponent,
                MaterialComponent = dto.MaterialComponent,
                Class = dto.Class,
                Description = dto.Description,
                PrintedCount = dto.PrintedCount,
            };

            dbContext.Spells.Add(entity);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to add spell from DTO.");
            throw;
        }
    }

    public async Task UpdateAsync(int id, SpellUpdateDto dto)
    {
        try
        {
            var entity = await dbContext.Spells.FindAsync(id);
            if (entity == null)
                return;

            if (!string.IsNullOrEmpty(dto.Name))
                entity.Name = dto.Name;
            if (!string.IsNullOrEmpty(dto.Type))
                entity.Type = dto.Type;
            if (dto.VerbalComponent.HasValue)
                entity.VerbalComponent = dto.VerbalComponent.Value;
            if (dto.SomaticComponent.HasValue)
                entity.SomaticComponent = dto.SomaticComponent.Value;
            if (dto.MaterialComponent.HasValue)
                entity.MaterialComponent = dto.MaterialComponent.Value;
            if (dto.Class.HasValue)
                entity.Class = dto.Class.Value;
            if (dto.Description != null)
                entity.Description = dto.Description;
            if (dto.PrintedCount.HasValue)
                entity.PrintedCount = dto.PrintedCount.Value;

            await dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to update spell with id {Id}.", id);
            throw;
        }
    }

    public async Task DeleteAsync(int id)
    {
        try
        {
            var entity = await dbContext.Spells.FindAsync(id);
            if (entity == null)
                return;

            dbContext.Spells.Remove(entity);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to delete spell with id {Id}.", id);
            throw;
        }
    }

    public Task<SpellEntity?> GetByIdAsync(int id)
        => dbContext.Spells.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);

    public async Task<IReadOnlyList<SpellEntity>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return await dbContext.Spells.AsNoTracking().ToListAsync();

        return await dbContext.Spells
            .AsNoTracking()
            .Where(e => EF.Functions.Like(e.Name, $"%{query}%"))
            .ToListAsync();
    }
}
