using FluentAssertions;
using InitiativeTracker.Application;
using InitiativeTracker.Domain.Entities;
using InitiativeTracker.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace InitiativeTracker.Tests;

public class InitiativeServiceRemoveTests
{
    private ILogger<InitiativeService> _logger;
    private InitiativeTrackerDbContext _dbContext;
    private InitiativeService _service;

    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<InitiativeService>>();
        var options = new DbContextOptionsBuilder<InitiativeTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"RemoveDb-{Guid.NewGuid()}")
            .Options;
        _dbContext = new InitiativeTrackerDbContext(options);
        _service = new InitiativeService(_logger, _dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Test]
    public void Remove_ShouldRemoveMatchingItem()
    {
        var item1 = CreateItem("Goblin", 15);
        var item2 = CreateItem("Orc", 20);
        _service.Append(item1);
        _service.Append(item2);

        _service.Remove(item1);

        _service.Items.Should().ContainSingle();
        _service.Items.First().Name.Should().Be("Orc");
    }

    [Test]
    public void Remove_WhenItemNotInList_ShouldDoNothing()
    {
        var item = CreateItem("Goblin", 15);
        _service.Append(item);

        _service.Remove(CreateItem("NonExistent", 5));

        _service.Items.Should().ContainSingle();
        _service.Items.First().Name.Should().Be("Goblin");
    }

    [Test]
    public void Remove_LastItem_ShouldWork()
    {
        var item1 = CreateItem("First", 10);
        var item2 = CreateItem("Second", 20);
        _service.Append(item1);
        _service.Append(item2);

        _service.Remove(item2);

        _service.Items.Should().ContainSingle();
        _service.Items.First().Name.Should().Be("First");
    }

    [Test]
    public void Remove_MiddleItem_ShouldReorder()
    {
        var item1 = CreateItem("A", 5);
        var item2 = CreateItem("B", 10);
        var item3 = CreateItem("C", 15);
        _service.AppendMultiple(new[] { item1, item2, item3 });

        _service.Remove(item2);

        var list = _service.Items.ToList();
        list[0].Name.Should().Be("A");
        list[1].Name.Should().Be("C");
    }

    [Test]
    public void Remove_SingleItem_ShouldEmptyList()
    {
        var item = CreateItem("Solo", 10);
        _service.Append(item);

        _service.Remove(item);

        _service.Items.Should().BeEmpty();
    }

    [Test]
    public void Remove_CurrentItem_WhenFirst_ShouldKeepCurrentAsIs()
    {
        var item1 = CreateItem("First", 10);
        var item2 = CreateItem("Second", 20);
        _service.Append(item1);
        _service.Append(item2);
        _service.Next();

        _service.Remove(item1);

        _service.Current.Should().BeSameAs(item1);
        _service.Items.Should().ContainSingle();
    }

    [Test]
    public void RemoveAt_IndexZero_ShouldRemoveFirst()
    {
        var item1 = CreateItem("First", 10);
        var item2 = CreateItem("Second", 20);
        _service.Append(item1);
        _service.Append(item2);

        _service.RemoveAt(0);

        _service.Items.Should().ContainSingle();
        _service.Items.First().Name.Should().Be("Second");
    }

    [Test]
    public void RemoveAt_IndexOne_ShouldRemoveSecond()
    {
        var item1 = CreateItem("First", 10);
        var item2 = CreateItem("Second", 20);
        _service.Append(item1);
        _service.Append(item2);

        _service.RemoveAt(1);

        _service.Items.Should().ContainSingle();
        _service.Items.First().Name.Should().Be("First");
    }

    [Test]
    public void RemoveAt_LastIndex_ShouldWork()
    {
        var items = new[]
        {
            CreateItem("A", 5),
            CreateItem("B", 10),
            CreateItem("C", 15)
        };
        _service.AppendMultiple(items);

        _service.RemoveAt(2);

        _service.Items.Should().HaveCount(2);
    }

    [Test]
    public void Clear_ShouldEmptyListAndResetState()
    {
        var items = new[]
        {
            CreateItem("A", 5),
            CreateItem("B", 10)
        };
        _service.AppendMultiple(items);
        _service.Next();
        _service.Next();

        _service.Clear();

        _service.Items.Should().BeEmpty();
        _service.Current.Should().BeNull();
        _service.CurrentRound.Should().Be(1);
    }

    static InitiativeListItem CreateItem(string name, int initiative) => new()
    {
        Name = name,
        Initiative = initiative,
        HitsDefault = 10,
        HitsCurrent = 10,
        ArmorClass = 12,
        ArmorClassCurrent = 12
    };
}
