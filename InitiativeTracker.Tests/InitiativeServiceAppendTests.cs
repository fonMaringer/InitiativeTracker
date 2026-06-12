using FluentAssertions;
using InitiativeTracker.Application;
using InitiativeTracker.Domain;
using InitiativeTracker.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace InitiativeTracker.Tests;

public class InitiativeServiceAppendTests
{
    private ILogger<InitiativeService> _logger;
    private InitiativeTrackerDbContext _dbContext;
    private InitiativeService _service;

    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<InitiativeService>>();
        var options = new DbContextOptionsBuilder<InitiativeTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"AppendDb-{Guid.NewGuid()}")
            .Options;
        _dbContext = new InitiativeTrackerDbContext(options);

        var provider = Substitute.For<IServiceProvider>();
        provider.GetService(typeof(InitiativeTrackerDbContext)).Returns(_dbContext);

        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(provider);

        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        scopeFactory.CreateScope().Returns(scope);

        provider.GetService(typeof(IServiceScopeFactory)).Returns(scopeFactory);

        _service = new InitiativeService(_logger, provider);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Test]
    public void Items_Initially_ShouldBeEmpty()
    {
        _service.Items.Should().BeEmpty();
    }

    [Test]
    public void Append_WhenEmpty_shouldAddSingleItem()
    {
        var item = CreateItem("Goblin", 15);
        _service.Append(item);

        _service.Items.Should().ContainEquivalentOf(item);
        _service.Items.Should().HaveCount(1);
    }

    [Test]
    public void AppendMultiple_shouldAddAllItems()
    {
        var items = new[]
        {
            CreateItem("Goblin", 15),
            CreateItem("Orc", 20),
            CreateItem("Troll", 25)
        };

        _service.AppendMultiple(items);

        _service.Items.Should().HaveCount(3);
    }

    [Test]
    public void Append_PreservesInsertionOrder()
    {
        var itemA = CreateItem("First", 10);
        var itemB = CreateItem("Second", 20);

        _service.Append(itemA);
        _service.Append(itemB);

        _service.Items.ToList()[0].Name.Should().Be("First");
        _service.Items.ToList()[1].Name.Should().Be("Second");
    }

    [Test]
    public void AppendMultiple_PreservesInsertionOrder()
    {
        var items = new[]
        {
            CreateItem("Alpha", 5),
            CreateItem("Beta", 10)
        };

        _service.AppendMultiple(items);

        _service.Items.ToList()[0].Name.Should().Be("Alpha");
        _service.Items.ToList()[1].Name.Should().Be("Beta");
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
