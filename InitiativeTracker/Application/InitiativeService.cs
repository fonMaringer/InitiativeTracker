using InitiativeTracker.Domain.Entities;
using InitiativeTracker.Domain.Enums;
using InitiativeTracker.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace InitiativeTracker.Application;

public record EncounterDto(int Id, string Name, DateTime CreatedAt);

public interface IInitiativeService
{
    int CurrentRound { get; }

    InitiativeListItem? Current { get; }

    IReadOnlyCollection<InitiativeListItem> Items { get; }

    int? ActiveEncounterId { get; }

    string? ActiveEncounterName { get; }

    IReadOnlyCollection<EncounterDto> AllEncounters { get; }

    Task ClearAsync();

    Task<int> CreateEncounterAsync(string name);

    Task DeleteEncounterAsync(int encounterId);

    void MoveAtDown(int index);

    void MoveAtUp(int index);

    void MoveDown(InitiativeListItem item);

    void MoveUp(InitiativeListItem item);

    void Next();

    void Append(InitiativeListItem item);

    void AppendMultiple(IEnumerable<InitiativeListItem> items);

    Task RenameEncounterAsync(int encounterId, string newName);

    void Remove(InitiativeListItem item);

    void RemoveAt(int index);

    void Restart();

    void SelectEncounter(int encounterId);

    void SortByInitiative();

    Task SaveAllAsync();

    Task WarmUpAsync();
}

public class InitiativeService(
    ILogger<InitiativeService> logger,
    InitiativeTrackerDbContext dbContext
) : IInitiativeService
{
    private Dictionary<int, EncounterState> _encounters = new();
    private int? _activeEncounterId;

    public int CurrentRound => ActiveState?.CurrentRound ?? 1;

    public InitiativeListItem? Current => ActiveState?.CurrentItem;

    public IReadOnlyCollection<InitiativeListItem> Items => ActiveState?.Items ?? [];

    public int? ActiveEncounterId => _activeEncounterId;

    public string? ActiveEncounterName =>
        _activeEncounterId.HasValue && _encounters.TryGetValue(_activeEncounterId.Value, out var state)
            ? state.EncounterName
            : null;

    public IReadOnlyCollection<EncounterDto> AllEncounters => _encounters.Values.Select(e => new EncounterDto(e.EncounterId, e.EncounterName, e.CreatedAt)).ToList();

    private EncounterState? ActiveState
    {
        get
        {
            if (_activeEncounterId.HasValue && _encounters.TryGetValue(_activeEncounterId.Value, out var state))
                return state;
            return null;
        }
    }

    public void Next()
    {
        var state = ActiveState;
        if (state is null)
            return;

        if (state.CurrentItem == null)
        {
            state.CurrentItem = state.Items.FirstOrDefault();
            return;
        }

        var currentIndex = _encounters[_activeEncounterId!.Value].Items.IndexOf(state.CurrentItem);
        var nextIndex = currentIndex + 1;
        if (nextIndex >= state.Items.Count)
        {
            state.CurrentRound++;
            nextIndex = 0;
        }

        state.CurrentItem = state.Items[nextIndex];
    }

    public void Restart()
    {
        var state = ActiveState;
        if (state is null)
            return;

        state.CurrentRound = 1;
        state.CurrentItem = state.Items.FirstOrDefault();
        foreach (var item in state.Items)
        {
            item.Reset();
        }
    }

    public void SortByInitiative()
    {
        var state = ActiveState;
        if (state is null)
            return;

        state.Items = state.Items.OrderByDescending(i => i.Initiative).ToList();
    }

    public async Task ClearAsync()
    {
        var state = ActiveState;
        if (state is null)
            return;

        state.CurrentRound = 1;
        state.CurrentItem = null;
        state.Items.Clear();
        await SaveEncounterAsync(state.EncounterId);
    }

    public void Append(InitiativeListItem item)
    {
        var state = ActiveState;
        if (state is null)
            return;

        state.Items.Add(item);
        _ = Task.Run(async () => await SaveEncounterAsync(state.EncounterId));
    }

    public void AppendMultiple(IEnumerable<InitiativeListItem> items)
    {
        var state = ActiveState;
        if (state is null)
            return;

        state.Items.AddRange(items);
        _ = Task.Run(async () => await SaveEncounterAsync(state.EncounterId));
    }

    public void Remove(InitiativeListItem item)
    {
        var state = ActiveState;
        if (state is null)
            return;

        state.Items.Remove(item);
        _ = Task.Run(async () => await SaveEncounterAsync(state.EncounterId));
    }

    public void RemoveAt(int index)
    {
        var state = ActiveState;
        if (state is null)
            return;

        state.Items.RemoveAt(index);
        _ = Task.Run(async () => await SaveEncounterAsync(state.EncounterId));
    }

    public void MoveUp(InitiativeListItem item) => MoveItem(item, true);
    public void MoveAtUp(int index)
    {
        var state = ActiveState;
        if (state is null)
            return;
        MoveItem(state.Items[index], true);
    }
    public void MoveDown(InitiativeListItem item) => MoveItem(item, false);
    public void MoveAtDown(int index)
    {
        var state = ActiveState;
        if (state is null)
            return;
        MoveItem(state.Items[index], false);
    }

    private void MoveItem(InitiativeListItem item, bool isUp)
    {
        var state = ActiveState;
        if (state is null)
            return;

        var oldIndex = state.Items.IndexOf(item);
        if (oldIndex <= -1)
            return;

        var newIndex = isUp ? oldIndex - 1 : oldIndex + 1;
        if (newIndex < 0)
            newIndex = 0;

        if (newIndex >= state.Items.Count)
            return;

        state.Items.RemoveAt(oldIndex);
        state.Items.Insert(newIndex, item);
    }

    public async Task WarmUpAsync()
    {
        try
        {
            var encountersMap = await dbContext.Encounters
                .ToDictionaryAsync(e => e.Id);

            var initiativeEntities = await dbContext.Initiatives
                .OrderBy(e => e.OrderIndex)
                .ToListAsync();

            foreach (var group in initiativeEntities.GroupBy(e => e.EncounterId))
            {
                if (encountersMap.TryGetValue(group.Key, out var encData))
                {
                    _encounters[group.Key] = new EncounterState(
                        EncounterId: group.Key,
                        EncounterName: encData.Name,
                        CreatedAt: encData.CreatedAt,
                        Items: group.Select(MapToItem).ToList(),
                        CurrentRound: 1,
                        CurrentItem: null
                    );
                }
            }

            if (!_encounters.Any())
            {
                _activeEncounterId = await CreateEncounterAsync("Default");
            }
            else if (_activeEncounterId == null || !_encounters.ContainsKey(_activeEncounterId.Value))
            {
                _activeEncounterId = _encounters.Keys.First();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to load initiatives from database.");
        }
    }

    public async Task SaveAllAsync()
    {
        foreach (var kvp in _encounters)
        {
            await SaveEncounterAsync(kvp.Key);
        }
    }

    private async Task SaveEncounterAsync(int encounterId)
    {
        if (!_encounters.TryGetValue(encounterId, out var state))
            return;

        try
        {
            var existing = dbContext.Initiatives.Where(e => e.EncounterId == encounterId).ToList();
            dbContext.Initiatives.RemoveRange(existing);

            var entities = state.Items.Select((item, index) => MapToEntity(item, encounterId, index)).ToList();
            dbContext.Initiatives.AddRange(entities);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to save initiative list for encounter {EncounterId}.", encounterId);
        }
    }

    private static InitiativeListItem MapToItem(InitiativeEntity entity)
    {
        return new InitiativeListItem
        {
            Name = entity.Name,
            Initiative = entity.Initiative,
            Dexterity = entity.Dexterity,
            HitsDefault = entity.HitsDefault,
            HitsCurrent = entity.HitsCurrent,
            ArmorClass = entity.ArmorClass,
            ArmorClassCurrent = entity.ArmorClassCurrent,
            Link = entity.Link,
            Source = entity.SourceId switch
            {
                nameof(Source.Manual) => Source.Manual,
                nameof(Source.Bestiary) => Source.Bestiary,
                _ => Source.Manual,
            },
        };
    }

    private static InitiativeEntity MapToEntity(InitiativeListItem item, int encounterId, int index)
    {
        return new InitiativeEntity
        {
            EncounterId = encounterId,
            Name = item.Name,
            Initiative = item.Initiative,
            Dexterity = item.Dexterity,
            HitsDefault = item.HitsDefault,
            HitsCurrent = item.HitsCurrent,
            ArmorClass = item.ArmorClass,
            ArmorClassCurrent = item.ArmorClassCurrent,
            Link = item.Link,
            SourceId = item.Source.ToString(),
            OrderIndex = index,
        };
    }

    public async Task<int> CreateEncounterAsync(string name)
    {
        var entity = new EncounterEntity
        {
            Name = name,
            CreatedAt = DateTime.UtcNow,
        };
        dbContext.Encounters.Add(entity);
        await dbContext.SaveChangesAsync();

        _encounters[entity.Id] = new EncounterState(
            EncounterId: entity.Id,
            EncounterName: entity.Name,
            CreatedAt: entity.CreatedAt,
            Items: [],
            CurrentRound: 1,
            CurrentItem: null
        );

        return entity.Id;
    }

    public async Task DeleteEncounterAsync(int encounterId)
    {
        if (!_encounters.Remove(encounterId))
            return;

        await dbContext.Encounters.Where(e => e.Id == encounterId).ExecuteDeleteAsync();

        if (_activeEncounterId == encounterId)
        {
            _activeEncounterId = _encounters.Any() ? _encounters.Keys.First() : null;
        }
    }

    public async Task RenameEncounterAsync(int encounterId, string newName)
    {
        var state = _encounters.GetValueOrDefault(encounterId);
        if (state is null)
            return;

        state.EncounterName = newName;

        await dbContext.Encounters
            .Where(e => e.Id == encounterId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(e => e.Name, newName));
    }

    public void SelectEncounter(int encounterId)
    {
        if (_encounters.ContainsKey(encounterId))
        {
            _activeEncounterId = encounterId;
        }
    }
}

internal sealed class EncounterState(
    int EncounterId,
    string EncounterName,
    DateTime CreatedAt,
    List<InitiativeListItem> Items,
    int CurrentRound,
    InitiativeListItem? CurrentItem
)
{
    public int EncounterId { get; } = EncounterId;
    public string EncounterName { get; set; } = EncounterName;
    public DateTime CreatedAt { get; } = CreatedAt;
    public List<InitiativeListItem> Items { get; set; } = Items;
    public int CurrentRound { get; set; } = CurrentRound;
    public InitiativeListItem? CurrentItem { get; set; } = CurrentItem;
}
