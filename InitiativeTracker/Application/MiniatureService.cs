using InitiativeTracker.Application.Dtos;
using InitiativeTracker.Domain.Entities;
using InitiativeTracker.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace InitiativeTracker.Application;

public interface IMiniatureService
{
    Task AddAsync(MiniatureCreateDto dto);
    Task UpdateAsync(int id, MiniatureUpdateDto dto);
    Task DeleteAsync(int id);
    Task<MiniatureEntity?> GetByIdAsync(int id);
    Task<IReadOnlyList<MiniatureEntity>> SearchAsync(string query);
    Task<byte[]> GetImageAsync(int miniatureId);
}

public class MiniatureService(
    ILogger<MiniatureService> logger,
    InitiativeTrackerDbContext dbContext
) : IMiniatureService
{
    public async Task AddAsync(MiniatureCreateDto dto)
    {
        try
        {
            var entity = new MiniatureEntity
            {
                Name = dto.Name,
                Size = dto.Size,
                ImageData = dto.ImageData,
                PrintedCount = dto.PrintedCount,
                Link = dto.Link,
                CropXOffset = dto.CropXOffset,
                CropYOffset = dto.CropYOffset,
                CropZoom = dto.CropZoom,
            };

            dbContext.Miniatures.Add(entity);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to add miniature from DTO.");
            throw;
        }
    }

    public async Task UpdateAsync(int id, MiniatureUpdateDto dto)
    {
        try
        {
            var entity = await dbContext.Miniatures.FindAsync(id);
            if (entity == null)
                return;

            if (!string.IsNullOrEmpty(dto.Name))
                entity.Name = dto.Name;
            if (dto.Size.HasValue)
                entity.Size = dto.Size.Value;
            if (dto.PrintedCount.HasValue)
                entity.PrintedCount = dto.PrintedCount.Value;
            entity.Link = dto.Link;
            if (dto.CropXOffset.HasValue)
                entity.CropXOffset = dto.CropXOffset.Value;
            if (dto.CropYOffset.HasValue)
                entity.CropYOffset = dto.CropYOffset.Value;
            if (dto.CropZoom.HasValue)
                entity.CropZoom = dto.CropZoom.Value;

            await dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to update miniature with id {Id}.", id);
            throw;
        }
    }

    public async Task DeleteAsync(int id)
    {
        try
        {
            var entity = await dbContext.Miniatures.FindAsync(id);
            if (entity == null)
                return;

            dbContext.Miniatures.Remove(entity);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to delete miniature with id {Id}.", id);
            throw;
        }
    }

    public Task<MiniatureEntity?> GetByIdAsync(int id)
        => dbContext.Miniatures.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);

    public async Task<IReadOnlyList<MiniatureEntity>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return await dbContext.Miniatures.AsNoTracking().ToListAsync();

        return await dbContext.Miniatures
            .AsNoTracking()
            .Where(e => EF.Functions.Like(e.Name, $"%{query}%"))
            .ToListAsync();
    }

    public async Task<byte[]> GetImageAsync(int miniatureId)
    {
        var imageData = await dbContext.Miniatures
            .AsNoTracking()
            .Where(e => e.Id == miniatureId)
            .Select(e => e.ImageData)
            .FirstOrDefaultAsync();

        return imageData ?? Array.Empty<byte>();
    }
}
