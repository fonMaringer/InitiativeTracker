using FluentAssertions;
using InitiativeTracker.Application;
using InitiativeTracker.Domain;
using InitiativeTracker.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace InitiativeTracker.Tests;

public class InitiativeServiceNextTests
{
    private ILogger<InitiativeService> _logger;
    private InitiativeTrackerDbContext _dbContext;
    private InitiativeService _service;

    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<InitiativeService>>();
        var options = new DbContextOptionsBuilder<InitiativeTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"NextDb-{Guid.NewGuid()}")
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
    public void Next_WhenEmpty_ShouldSetCurrentToNull()
    {
        _service.Next();
        _service.Current.Should().BeNull();
    }

    [Test]
    public void Next_FirstCallWithItems_ShouldSetCurrentToFirst()
    {
        var item = new InitiativeListItem { Name = "Goblin", Initiative = 12 };
        _service.Append(item);
        _service.Next();
        _service.Current.Should().BeSameAs(item);
    }

    [Test]
    public void Next_SecondCall_ShouldMoveToSecondItem()
    {
        var item1 = new InitiativeListItem { Name = "Goblin", Initiative = 5 };
        var item2 = new InitiativeListItem { Name = "Orc", Initiative = 10 };
        _service.Append(item1);
        _service.Append(item2);
        _service.Next();
        _service.Next();
        _service.Current.Should().BeSameAs(item2);
    }

    [Test]
    public void Next_CyclingPastEnd_ShouldWrapToFirstAndIncrementRound()
    {
        var item1 = new InitiativeListItem { Name = "Goblin" };
        var item2 = new InitiativeListItem { Name = "Orc" };
        _service.Append(item1);
        _service.Append(item2);
        _service.Next();
        _service.Next();
        _service.Next();
        _service.Current.Should().BeSameAs(item1);
        _service.CurrentRound.Should().Be(2);
    }

    [Test]
    public void Next_MultipleCycles_ShouldIncrementRoundCorrectly()
    {
        var item1 = new InitiativeListItem { Name = "Goblin" };
        var item2 = new InitiativeListItem { Name = "Orc" };
        _service.Append(item1);
        _service.Append(item2);

        for (int i = 0; i < 6; i++)
        {
            _service.Next();
        }

        _service.Current.Should().BeSameAs(item2);
        _service.CurrentRound.Should().Be(3);
    }

    [Test]
    public void Next_SingleItem_ShouldWrapToSameAndIncrementRound()
    {
        var item = new InitiativeListItem { Name = "Solo", Initiative = 5 };
        _service.Append(item);
        _service.Next();
        _service.Current.Should().BeSameAs(item);
        _service.CurrentRound.Should().Be(1);

        _service.Next();
        _service.Current.Should().BeSameAs(item);
        _service.CurrentRound.Should().Be(2);
    }
}
