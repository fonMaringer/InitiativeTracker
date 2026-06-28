using InitiativeTracker.DataAccess.Dtos;
using InitiativeTracker.Domain.Entities;
using InitiativeTracker.Infrastructure;
using InitiativeTracker.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace InitiativeTracker.DataAccess.Repositories;

public interface IStandRepository : IRepository
{
    Task AddAsync(StandCreateDto dto);
    Task UpdateAsync(int id, StandUpdateDto dto);
    Task DeleteAsync(int id);
    Task<Stand?> GetByIdAsync(int id);
    Task<IReadOnlyCollection<Stand>> GetAllAsync();
    Task<byte[]> GetImageAsync(int standId);
}

public class StandRepository(
    ILogger<StandRepository> logger,
    InitiativeTrackerDbContext dbContext
) : IStandRepository
{
    public async Task AddAsync(StandCreateDto dto)
    {
        try
        {
            var entity = new Stand
            {
                ImageData = dto.ImageData,
                InverseTextColor = dto.InverseTextColor,
            };

            dbContext.Stands.Add(entity);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to add stand from DTO.");
            throw;
        }
    }

    public async Task UpdateAsync(int id, StandUpdateDto dto)
    {
        try
        {
            var entity = await dbContext.Stands.FindAsync(id);
            if (entity == null)
                return;

            entity.ImageData = dto.ImageData;
            entity.InverseTextColor = dto.InverseTextColor;

            await dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to update stand with id {Id}.", id);
            throw;
        }
    }

    public async Task DeleteAsync(int id)
    {
        try
        {
            var entity = await dbContext.Stands.FindAsync(id);
            if (entity == null)
                return;

            dbContext.Stands.Remove(entity);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to delete stand with id {Id}.", id);
            throw;
        }
    }

    public Task<Stand?> GetByIdAsync(int id)
        => dbContext.Stands.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);

    public async Task<IReadOnlyCollection<Stand>> GetAllAsync()
    {
        return await dbContext.Stands
            .OrderBy(p => p.Id)
            .ToListAsync();
    }

    public async Task<byte[]> GetImageAsync(int standId)
    {
        var imageData = await dbContext.Stands
            .AsNoTracking()
            .Where(e => e.Id == standId)
            .Select(e => e.ImageData)
            .FirstOrDefaultAsync();

        return imageData ?? [];
    }
}
