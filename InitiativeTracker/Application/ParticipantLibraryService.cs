using Microsoft.EntityFrameworkCore;

namespace InitiativeTracker.Application;

public record GlobalParticipantDto(
    int Id,
    string Name,
    int Hp,
    int Ac,
    int Dexterity
);

public record CreateParticipantDto(
    string Name,
    int Hp,
    int Ac,
    int Dexterity
);

public interface IParticipantLibraryService
{
    Task<IReadOnlyCollection<GlobalParticipantDto>> GetAllAsync();

    Task<GlobalParticipantDto?> GetByIdAsync(int id);

    Task<int> CreateAsync(CreateParticipantDto dto);

    Task RenameAsync(int id, string newName);

    Task DeleteAsync(int id);
}

public class ParticipantLibraryService(
    ILogger<ParticipantLibraryService> logger,
    Infrastructure.Database.InitiativeTrackerDbContext dbContext
) : IParticipantLibraryService
{
    public async Task<IReadOnlyCollection<GlobalParticipantDto>> GetAllAsync()
    {
        return await dbContext.GlobalParticipants
            .OrderBy(p => p.Name)
            .Select(e => new GlobalParticipantDto(
                Id: e.Id,
                Name: e.Name,
                Hp: e.Hp,
                Ac: e.Ac,
                Dexterity: e.Dexterity
            ))
            .ToListAsync();
    }

    public async Task<GlobalParticipantDto?> GetByIdAsync(int id)
    {
        var participant = await dbContext.GlobalParticipants.FindAsync(id);
        return participant is null ? null : MapToDto(participant);
    }

    public async Task<int> CreateAsync(CreateParticipantDto dto)
    {
        try
        {
            var entity = new Domain.Entities.GlobalParticipantEntity
            {
                Name = dto.Name,
                Dexterity = dto.Dexterity,
                Hp = dto.Hp,
                Ac = dto.Ac
            };

            dbContext.GlobalParticipants.Add(entity);
            await dbContext.SaveChangesAsync();
            return entity.Id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to create participant \"{Name}\".", dto.Name);
            throw;
        }
    }

    public async Task RenameAsync(int id, string newName)
    {
        try
        {
            await dbContext.GlobalParticipants
                .Where(p => p.Id == id)
                .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.Name, newName));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to rename participant with Id {Id}.", id);
            throw;
        }
    }

    public async Task DeleteAsync(int id)
    {
        try
        {
            await dbContext.GlobalParticipants
                .Where(p => p.Id == id)
                .ExecuteDeleteAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to delete participant with Id {Id}.", id);
            throw;
        }
    }

    private static GlobalParticipantDto MapToDto(Domain.Entities.GlobalParticipantEntity entity)
    {
        return new GlobalParticipantDto(
            Id: entity.Id,
            Name: entity.Name,
            Hp: entity.Hp,
            Ac: entity.Ac,
            Dexterity: entity.Dexterity
        );
    }
}
