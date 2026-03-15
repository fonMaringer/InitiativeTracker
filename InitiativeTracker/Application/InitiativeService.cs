using System.Reflection;
using System.Text.Json;
using InitiativeTracker.Domain;

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
    ILogger<InitiativeService> logger
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

    private const string fileName = "InitiativeList.json";

    public void WarmUp()
    {
        var filePath = GetFilePath();
        if (!File.Exists(filePath))
            return;

        try
        {
            var json = File.ReadAllText(filePath);
            _items = JsonSerializer.Deserialize<List<InitiativeListItem>>(json)!;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to deserialize initiative list file: {FilePath}.", filePath);
        }
    }

    public void SaveToFile()
    {
        var filePath = GetFilePath();
        try
        {
            if (File.Exists(filePath))
                File.Delete(filePath);

            var json = JsonSerializer.Serialize(_items, new JsonSerializerOptions
            {
                WriteIndented = true,
            });
            File.WriteAllText(filePath, json);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to serialize initiative list to file: {FilePath}.", filePath);
        }
            
    }

    private static string GetFilePath()
    {
        var currentFilePath = Assembly.GetExecutingAssembly().Location;
        return Path.Combine(Path.GetDirectoryName(currentFilePath), fileName);
    }
}