using FluentAssertions;
using InitiativeTracker.Application;
using InitiativeTracker.Domain;
using InitiativeTracker.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace InitiativeTracker.Tests;

public class InitiativeServiceMoveTests
{
    private ILogger<InitiativeService> _logger;
    private InitiativeTrackerDbContext _dbContext;
    private InitiativeService _service;

    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<InitiativeService>>();
        var options = new DbContextOptionsBuilder<InitiativeTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"MoveDb-{Guid.NewGuid()}")
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
    public void MoveUp_EmptyList_ShouldDoNothing()
    {
        var item = new InitiativeListItem { Name = "Solo", Initiative = 5 };

        _service.MoveUp(item);

        _service.Items.Should().BeEmpty();
    }

    [Test]
    public void MoveUp_ItemNotInList_ShouldDoNothing()
    {
        var inList = new InitiativeListItem { Name = "InList", Initiative = 5 };
        var notInList = new InitiativeListItem { Name = "NotInList", Initiative = 10 };

        _service.Append(inList);
        _service.MoveUp(notInList);

        _service.Items.Should().ContainSingle(x => x.Name == "InList");
    }

    [Test]
    public void MoveUp_FirstItem_ShouldStayAtTop()
    {
        var first = new InitiativeListItem { Name = "First", Initiative = 20 };
        var second = new InitiativeListItem { Name = "Second", Initiative = 15 };

        _service.Append(first);
        _service.Append(second);

        _service.MoveUp(first);

        var list = _service.Items.ToList();
        list[0].Name.Should().Be("First");
        list[1].Name.Should().Be("Second");
    }

    [Test]
    public void MoveUp_OnePosition_ShouldSwapWithPrevious()
    {
        var first = new InitiativeListItem { Name = "First", Initiative = 20 };
        var second = new InitiativeListItem { Name = "Second", Initiative = 15 };
        var third = new InitiativeListItem { Name = "Third", Initiative = 10 };

        _service.Append(first);
        _service.Append(second);
        _service.Append(third);

        _service.MoveUp(second);

        var list = _service.Items.ToList();
        list[0].Name.Should().Be("Second");
        list[1].Name.Should().Be("First");
        list[2].Name.Should().Be("Third");
    }

    [Test]
    public void MoveUp_LastItem_ToTop_MultipleMoves()
    {
        var items = new[]
        {
            new InitiativeListItem { Name = "A", Initiative = 10 },
            new InitiativeListItem { Name = "B", Initiative = 20 },
            new InitiativeListItem { Name = "C", Initiative = 30 }
        };

        _service.AppendMultiple(items);

        for (int i = 0; i < 5; i++)
        {
            _service.MoveUp(items[2]);
        }

        var list = _service.Items.ToList();
        list[0].Name.Should().Be("C");
    }

    [Test]
    public void MoveAtUp_FirstItem_ShouldStayAtTop()
    {
        var first = new InitiativeListItem { Name = "First", Initiative = 20 };
        var second = new InitiativeListItem { Name = "Second", Initiative = 15 };

        _service.Append(first);
        _service.Append(second);

        _service.MoveAtUp(0);

        var list = _service.Items.ToList();
        list[0].Name.Should().Be("First");
        list[1].Name.Should().Be("Second");
    }

    [Test]
    public void MoveAtUp_ValidIndex_ShouldMoveUp()
    {
        var items = new[]
        {
            new InitiativeListItem { Name = "A", Initiative = 10 },
            new InitiativeListItem { Name = "B", Initiative = 20 },
            new InitiativeListItem { Name = "C", Initiative = 30 }
        };

        _service.AppendMultiple(items);
        _service.MoveAtUp(2);

        var list = _service.Items.ToList();
        list[1].Name.Should().Be("C");
        list[2].Name.Should().Be("B");
    }

    [Test]
    public void MoveDown_EmptyList_ShouldDoNothing()
    {
        var item = new InitiativeListItem { Name = "Solo", Initiative = 5 };

        _service.MoveDown(item);

        _service.Items.Should().BeEmpty();
    }

    [Test]
    public void MoveDown_ItemNotInList_ShouldDoNothing()
    {
        var inList = new InitiativeListItem { Name = "InList", Initiative = 5 };
        var notInList = new InitiativeListItem { Name = "NotInList", Initiative = 10 };

        _service.Append(inList);
        _service.MoveDown(notInList);

        _service.Items.Should().ContainSingle(x => x.Name == "InList");
    }

    [Test]
    public void MoveDown_LastItem_ShouldStayAtBottom()
    {
        var first = new InitiativeListItem { Name = "First", Initiative = 20 };
        var second = new InitiativeListItem { Name = "Second", Initiative = 15 };

        _service.Append(first);
        _service.Append(second);

        _service.MoveDown(second);

        var list = _service.Items.ToList();
        list[0].Name.Should().Be("First");
        list[1].Name.Should().Be("Second");
    }

    [Test]
    public void MoveDown_OnePosition_ShouldSwapWithNext()
    {
        var first = new InitiativeListItem { Name = "First", Initiative = 20 };
        var second = new InitiativeListItem { Name = "Second", Initiative = 15 };
        var third = new InitiativeListItem { Name = "Third", Initiative = 10 };

        _service.Append(first);
        _service.Append(second);
        _service.Append(third);

        _service.MoveDown(second);

        var list = _service.Items.ToList();
        list[0].Name.Should().Be("First");
        list[1].Name.Should().Be("Third");
        list[2].Name.Should().Be("Second");
    }

    [Test]
    public void MoveDown_FirstItem_ToBottom_MultipleMoves()
    {
        var items = new[]
        {
            new InitiativeListItem { Name = "A", Initiative = 10 },
            new InitiativeListItem { Name = "B", Initiative = 20 },
            new InitiativeListItem { Name = "C", Initiative = 30 }
        };

        _service.AppendMultiple(items);

        for (int i = 0; i < 5; i++)
        {
            _service.MoveDown(items[0]);
        }

        var list = _service.Items.ToList();
        list[^1].Name.Should().Be("A");
    }

    [Test]
    public void MoveAtDown_LastItem_ShouldStayAtBottom()
    {
        var first = new InitiativeListItem { Name = "First", Initiative = 20 };
        var second = new InitiativeListItem { Name = "Second", Initiative = 15 };

        _service.Append(first);
        _service.Append(second);

        _service.MoveAtDown(1);

        var list = _service.Items.ToList();
        list[0].Name.Should().Be("First");
        list[1].Name.Should().Be("Second");
    }

    [Test]
    public void MoveAtDown_ValidIndex_ShouldMoveDown()
    {
        var items = new[]
        {
            new InitiativeListItem { Name = "A", Initiative = 10 },
            new InitiativeListItem { Name = "B", Initiative = 20 },
            new InitiativeListItem { Name = "C", Initiative = 30 }
        };

        _service.AppendMultiple(items);
        _service.MoveAtDown(0);

        var list = _service.Items.ToList();
        list[0].Name.Should().Be("B");
        list[1].Name.Should().Be("A");
    }

    [Test]
    public void MoveUp_SingleItem_ShouldDoNothing()
    {
        var item = new InitiativeListItem { Name = "Solo", Initiative = 5 };
        _service.Append(item);

        _service.MoveUp(item);

        _service.Items.Should().ContainSingle(x => x.Name == "Solo");
    }

    [Test]
    public void MoveDown_SingleItem_ShouldDoNothing()
    {
        var item = new InitiativeListItem { Name = "Solo", Initiative = 5 };
        _service.Append(item);

        _service.MoveDown(item);

        _service.Items.Should().ContainSingle(x => x.Name == "Solo");
    }

    [Test]
    public void MoveUp_MultipleItems_ComplexReordering()
    {
        var items = new[]
        {
            new InitiativeListItem { Name = "A", Initiative = 10 },
            new InitiativeListItem { Name = "B", Initiative = 20 },
            new InitiativeListItem { Name = "C", Initiative = 30 },
            new InitiativeListItem { Name = "D", Initiative = 40 }
        };

        _service.AppendMultiple(items);

        _service.MoveUp(items[3]);
        var list = _service.Items.ToList();
        list[0].Name.Should().Be("A");
        list[1].Name.Should().Be("B");
        list[2].Name.Should().Be("D");
        list[3].Name.Should().Be("C");

        _service.MoveUp(items[3]);
        list = _service.Items.ToList();
        list[0].Name.Should().Be("A");
        list[1].Name.Should().Be("D");
        list[2].Name.Should().Be("B");
        list[3].Name.Should().Be("C");
    }

    [Test]
    public void MoveDown_MultipleItems_ComplexReordering()
    {
        var items = new[]
        {
            new InitiativeListItem { Name = "A", Initiative = 10 },
            new InitiativeListItem { Name = "B", Initiative = 20 },
            new InitiativeListItem { Name = "C", Initiative = 30 },
            new InitiativeListItem { Name = "D", Initiative = 40 }
        };

        _service.AppendMultiple(items);

        _service.MoveDown(items[0]);
        var list = _service.Items.ToList();
        list[0].Name.Should().Be("B");
        list[1].Name.Should().Be("A");
        list[2].Name.Should().Be("C");
        list[3].Name.Should().Be("D");

        _service.MoveDown(items[0]);
        list = _service.Items.ToList();
        list[0].Name.Should().Be("B");
        list[1].Name.Should().Be("C");
        list[2].Name.Should().Be("A");
        list[3].Name.Should().Be("D");
    }
}
