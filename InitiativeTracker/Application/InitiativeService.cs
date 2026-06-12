using InitiativeTracker.Domain;
using InitiativeTracker.Domain.Entities;
using InitiativeTracker.Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;

namespace InitiativeTracker.Application;

public interface IInitiativeService
{
    int CurrentRound { get; }

    InitiativeListItem? Current { get; }

    IReadOnlyCollection<InitiativeListItem> Items { get; }

    void Next();

    void Restart();

    void SortByInitiative();

    void Remove(InitiativeListItem item);
    void RemoveAt(int index);

    void MoveUp(InitiativeListItem item);
    void MoveAtUp(int index);
    void MoveDown(InitiativeListItem item);
    void MoveAtDown(int index);

    void Clear();

    void Append(InitiativeListItem item);
    void AppendMultiple(IEnumerable<InitiativeListItem> items);

    void WarmUp();
    void SaveToFile();
}

public class InitiativeService(
    ILogger<InitiativeService> logger,
    IServiceProvider serviceProvider
) : IInitiativeService
{
    private List<InitiativeListItem> _items = [];

    public int CurrentRound { get; private set; } = 1;

    public InitiativeListItem? Current { get; private set; }

    public IReadOnlyCollection<InitiativeListItem> Items => _items;

    public void Next()
    {
        if (Current == null)
        {
            Current = Items.FirstOrDefault();
            return;
        }

        var currentIndex = _items.IndexOf(Current);
        var nextIndex = currentIndex + 1;
        if (nextIndex >= _items.Count)
        {
            CurrentRound++;
            nextIndex = 0;
        }

        Current = _items[nextIndex];
    }

    public void Restart()
    {
        CurrentRound = 1;
        Current = Items.FirstOrDefault();
        foreach (var item in Items)
        {
            item.Reset();
        }
    }

    public void SortByInitiative() => _items = _items.OrderByDescending(i => i.Initiative).ToList();

    public void Clear()
    {
        CurrentRound = 1;
        Current = null;
        _items.Clear();
    }

    public void Append(InitiativeListItem item) => _items.Add(item);

    public void AppendMultiple(IEnumerable<InitiativeListItem> items) => _items.AddRange(items);

    public void Remove(InitiativeListItem item) => _items.Remove(item);

    public void RemoveAt(int index) => _items.RemoveAt(index);

    public void MoveUp(InitiativeListItem item) => MoveItem(item, true);
    public void MoveAtUp(int index) => MoveItem(_items[index], true);
    public void MoveDown(InitiativeListItem item) => MoveItem(item, false);
    public void MoveAtDown(int index) => MoveItem(_items[index], false);

    private void MoveItem(InitiativeListItem item, bool isUp)
    {
        var oldIndex = _items.IndexOf(item);

        if (oldIndex <= -1)
            return;

        var newIndex = isUp ? oldIndex - 1 : oldIndex + 1;
        if (newIndex < 0)
        {
            newIndex = 0;
        }

        if (newIndex >= _items.Count)
        {
            return;
        }

        _items.RemoveAt(oldIndex);

        _items.Insert(newIndex, item);
    }

    public void WarmUp()
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<InitiativeTrackerDbContext>();
            var entities = ctx.Initiatives.OrderBy(e => e.OrderIndex).ToList();
            _items = entities.Select(MapToItem).ToList();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to load initiative list from database.");
        }
    }

    public void SaveToFile()
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<InitiativeTrackerDbContext>();
            ctx.Initiatives.RemoveRange(ctx.Initiatives.ToList());
            var entities = _items.Select((item, index) => MapToEntity(item, index)).ToList();
            ctx.Initiatives.AddRange(entities);
            ctx.SaveChanges();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to save initiative list to database.");
        }
    }

    static InitiativeListItem MapToItem(InitiativeEntity entity)
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

    static InitiativeEntity MapToEntity(InitiativeListItem item, int index)
    {
        return new InitiativeEntity
        {
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
}