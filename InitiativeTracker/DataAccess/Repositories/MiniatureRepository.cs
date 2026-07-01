using InitiativeTracker.DataAccess.Dtos;
using InitiativeTracker.Domain.Entities;
using InitiativeTracker.Infrastructure;
using InitiativeTracker.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace InitiativeTracker.DataAccess.Repositories;

public interface IMiniatureRepository : IRepository
{
    Task AddAsync(MiniatureCreateDto dto);
    Task UpdateAsync(int id, MiniatureUpdateDto dto);
    Task DeleteAsync(int id);
    Task<Miniature?> GetByIdAsync(int id);
    Task<IReadOnlyList<Miniature>> SearchAsync(string query);
    Task<byte[]> GetImageAsync(int miniatureId);
}

public class MiniatureRepository(
    ILogger<MiniatureRepository> logger,
    InitiativeTrackerDbContext dbContext
) : IMiniatureRepository
{
    public async Task AddAsync(MiniatureCreateDto dto)
    {
        try
        {
            var entity = new Miniature
            {
                Name = dto.Name,
                Size = dto.Size,
                ImageData = dto.ImageData,
                CroppedImageData = dto.CroppedImageData,
                PrintedCount = dto.PrintedCount,
                Link = dto.Link,
                CropX = dto.CropX,
                CropY = dto.CropY,
                CropWidth = dto.CropWidth,
                CropHeight = dto.CropHeight,
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
            entity.ImageData = dto.ImageData;
            entity.CroppedImageData = dto.CroppedImageData;
            entity.Link = dto.Link;
            entity.CropX = dto.CropX;
            entity.CropY = dto.CropY;
            entity.CropWidth = dto.CropWidth;
            entity.CropHeight = dto.CropHeight;

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

    public Task<Miniature?> GetByIdAsync(int id)
        => dbContext.Miniatures.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);

    public async Task<IReadOnlyList<Miniature>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return await dbContext.Miniatures
                .AsNoTracking()
                .OrderBy(m => m.Name)
                .ToListAsync();

        query = query.Trim();

        return await dbContext.Miniatures
            .AsNoTracking()
            .OrderBy(m => m.Name)
            .AsAsyncEnumerable()
            .Where(e => e.Name.Contains(query, StringComparison.InvariantCultureIgnoreCase))
            .ToListAsync();
    }

    public async Task<byte[]> GetImageAsync(int miniatureId)
    {
        var imageData = await dbContext.Miniatures
            .AsNoTracking()
            .Where(e => e.Id == miniatureId)
            .Select(e => e.ImageData)
            .FirstOrDefaultAsync();

        return imageData ?? [];
    }
}
