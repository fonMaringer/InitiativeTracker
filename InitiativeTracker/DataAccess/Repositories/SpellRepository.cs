using InitiativeTracker.DataAccess.Dtos;
using InitiativeTracker.Domain.Entities;
using InitiativeTracker.Infrastructure;
using InitiativeTracker.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace InitiativeTracker.DataAccess.Repositories;

public interface ISpellRepository : IRepository
{
    Task AddAsync(SpellCreateDto dto);
    Task UpdateAsync(int id, SpellUpdateDto dto);
    Task DeleteAsync(int id);
    Task<Spell?> GetByIdAsync(int id);
    Task<IReadOnlyList<Spell>> SearchAsync(string query);
}

public class SpellRepository(
    ILogger<SpellRepository> logger,
    InitiativeTrackerDbContext dbContext
) : ISpellRepository
{
    public async Task AddAsync(SpellCreateDto dto)
    {
        try
        {
            var entity = new Spell
            {
                Name = dto.Name,
                Type = dto.Type,
                VerbalComponent = dto.VerbalComponent,
                SomaticComponent = dto.SomaticComponent,
                MaterialComponent = dto.MaterialComponent,
                Classes = dto.Classes,
                Subclasses = dto.Subclasses,
                Description = dto.Description,
                PrintedCount = dto.PrintedCount,
                Link = dto.Link,
                Duration = dto.Duration,
                Range = dto.Range,
                Time = dto.Time,
                Concentration = dto.Concentration,
                Level = dto.Level,
                Upper = dto.Upper,
                Source = dto.Source,
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
            if (!string.IsNullOrEmpty(dto.MaterialComponent))
                entity.MaterialComponent = dto.MaterialComponent;
            if (dto.Classes is { Length: > 0 })
                entity.Classes = dto.Classes;
            if (dto.Subclasses is { Length: > 0 })
                entity.Subclasses = dto.Subclasses;
            if (dto.Description != null)
                entity.Description = dto.Description;
            if (dto.PrintedCount.HasValue)
                entity.PrintedCount = dto.PrintedCount.Value;
            entity.Link = dto.Link;

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

    public Task<Spell?> GetByIdAsync(int id)
        => dbContext.Spells.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);

    public async Task<IReadOnlyList<Spell>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return await dbContext.Spells
                .AsNoTracking()
                .OrderBy(m => m.Name)
                .ToListAsync();

        return await dbContext.Spells
            .AsNoTracking()
            .Where(e => EF.Functions.Like(e.Name, $"%{query}%"))
            .OrderBy(m => m.Name)
            .ToListAsync();
    }
}
