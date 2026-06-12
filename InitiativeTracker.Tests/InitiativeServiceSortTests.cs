using FluentAssertions;
using InitiativeTracker.Application;
using InitiativeTracker.Domain;
using InitiativeTracker.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace InitiativeTracker.Tests;

public class InitiativeServiceSortTests
{
    private ILogger<InitiativeService> _logger;
    private InitiativeTrackerDbContext _dbContext;
    private InitiativeService _service;

    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<InitiativeService>>();
        var options = new DbContextOptionsBuilder<InitiativeTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"SortDb-{Guid.NewGuid()}")
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
    public void SortByInitiative_EmptyList_ShouldNotChange()
    {
        _service.SortByInitiative();

        _service.Items.Should().BeEmpty();
    }

    [Test]
    public void SortByInitiative_SingleItem_ShouldNotChange()
    {
        var item = new InitiativeListItem { Name = "Solo", Initiative = 5 };
        _service.Append(item);

        _service.SortByInitiative();

        _service.Items.Should().ContainSingle(x => x.Name == "Solo");
    }

    [Test]
    public void SortByInitiative_TwoItems_ShouldSortDescending()
    {
        var low = new InitiativeListItem { Name = "Low", Initiative = 5 };
        var high = new InitiativeListItem { Name = "High", Initiative = 20 };

        _service.Append(low);
        _service.Append(high);

        _service.SortByInitiative();

        var list = _service.Items.ToList();
        list[0].Name.Should().Be("High");
        list[1].Name.Should().Be("Low");
    }

    [Test]
    public void SortByInitiative_MultipleItems_ShouldSortDescending()
    {
        var a = new InitiativeListItem { Name = "A", Initiative = 10 };
        var b = new InitiativeListItem { Name = "B", Initiative = 25 };
        var c = new InitiativeListItem { Name = "C", Initiative = 15 };

        _service.Append(a);
        _service.Append(b);
        _service.Append(c);

        _service.SortByInitiative();

        var list = _service.Items.ToList();
        list[0].Name.Should().Be("B");
        list[1].Name.Should().Be("C");
        list[2].Name.Should().Be("A");
    }

    [Test]
    public void SortByInitiative_AlreadySorted_ShouldKeepOrder()
    {
        var high = new InitiativeListItem { Name = "High", Initiative = 30 };
        var mid = new InitiativeListItem { Name = "Mid", Initiative = 20 };
        var low = new InitiativeListItem { Name = "Low", Initiative = 10 };

        _service.Append(high);
        _service.Append(mid);
        _service.Append(low);

        _service.SortByInitiative();

        var list = _service.Items.ToList();
        list[0].Name.Should().Be("High");
        list[1].Name.Should().Be("Mid");
        list[2].Name.Should().Be("Low");
    }

    [Test]
    public void SortByInitiative_SameInitiative_ShouldKeepInsertionOrder()
    {
        var first = new InitiativeListItem { Name = "First", Initiative = 15 };
        var second = new InitiativeListItem { Name = "Second", Initiative = 15 };
        var third = new InitiativeListItem { Name = "Third", Initiative = 15 };

        _service.Append(first);
        _service.Append(second);
        _service.Append(third);

        _service.SortByInitiative();

        var list = _service.Items.ToList();
        list[0].Name.Should().Be("First");
        list[1].Name.Should().Be("Second");
        list[2].Name.Should().Be("Third");
    }

    [Test]
    public void SortByInitiative_CurrentItem_ShouldNotBeAffected()
    {
        var a = new InitiativeListItem { Name = "A", Initiative = 10 };
        var b = new InitiativeListItem { Name = "B", Initiative = 20 };

        _service.Append(a);
        _service.Append(b);
        _service.Next();
        _service.Current.Should().BeSameAs(a);

        _service.SortByInitiative();

        _service.Current.Should().BeSameAs(a);
    }
}
