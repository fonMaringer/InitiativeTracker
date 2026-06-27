using Bunit;
using FluentAssertions;
using InitiativeTracker.DataAccess.Repositories;
using InitiativeTracker.Domain.Entities;
using InitiativeTracker.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace InitiativeTracker.Tests.HandlerTests;

public class InitiativeServiceMoveTests
{
    private BunitContext _ctx;
    
    private IEncounterRepository _encounterRepository;
    private IEncounterParticipantsRepository _participantsRepository;
    private Encounter _encounter;

    [SetUp]
    public void Setup()
    {
        _ctx = new BunitContext();
        
        _encounterRepository = Substitute.For<IEncounterRepository>();
        _participantsRepository = Substitute.For<IEncounterParticipantsRepository>();
        _ctx.Services.AddSingleton(_encounterRepository);
        _ctx.Services.AddSingleton(_participantsRepository);
        _encounter = new Encounter { Id = 1, Name = "Boss Fight", CurrentRound = 1 };
    }

    [TearDown]
    public void TearDown()
    {
        _ctx.Dispose();
    }

    [Test]
    public async Task MoveUp_EmptyList_ShouldDoNothing()
    {
        //Arrange
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<EncounterParticipant>>([]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));

        //Act + Assert
        var item = CreateItem(999, "Solo", 5);
        await component.InvokeAsync(async () => await component.Instance.Move(item.Id, isUp: true));

        var state = component.Instance.Participants;
        state.Should().BeEmpty();
    }

    [Test]
    public async Task MoveUp_ItemNotInList_ShouldDoNothing()
    {
        //Arrange
        var inList = CreateItem(100, "InList", 5);
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<EncounterParticipant>>([inList]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));

        //Act
        await component.InvokeAsync(async () => await component.Instance.Move(999, isUp: true));

        //Assert
        var state = component.Instance.Participants;
        state.Should().ContainSingle(x => x.Name == "InList");
    }

    [Test]
    public async Task MoveUp_FirstItem_ShouldStayAtTop()
    {
        //Arrange
        var first = CreateItem(100, "First", 20);
        var second = CreateItem(101, "Second", 15);
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<EncounterParticipant>>([first, second]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));

        //Act
        await component.InvokeAsync(async () => await component.Instance.Move(first.Id, isUp: true));

        //Assert
        var list = component.Instance.Participants;
        list[0].Name.Should().Be("First");
        list[1].Name.Should().Be("Second");
    }

    [Test]
    public async Task MoveUp_OnePosition_ShouldSwapWithPrevious()
    {
        //Arrange
        var first = CreateItem(100, "First", 20);
        var second = CreateItem(101, "Second", 15);
        var third = CreateItem(102, "Third", 10);
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<EncounterParticipant>>([first, second, third]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));

        //Act
        await component.InvokeAsync(async () => await component.Instance.Move(second.Id, isUp: true));

        //Assert
        var list = component.Instance.Participants;
        list[0].Name.Should().Be("Second");
        list[1].Name.Should().Be("First");
        list[2].Name.Should().Be("Third");
    }

    [Test]
    public async Task MoveUp_LastItem_ToTop_MultipleMoves()
    {
        //Arrange
        var a = CreateItem(100, "A", 10);
        var b = CreateItem(101, "B", 20);
        var c = CreateItem(102, "C", 30);
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<EncounterParticipant>>([a, b, c]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));

        //Act
        for (int i = 0; i < 5; i++)
        {
            await component.InvokeAsync(async () => await component.Instance.Move(c.Id, isUp: true));
        }

        //Assert
        var list = component.Instance.Participants;
        list[0].Name.Should().Be("C");
    }

    [Test]
    public async Task MoveDown_EmptyList_ShouldDoNothing()
    {
        //Arrange
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<EncounterParticipant>>([]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));

        //Act
        await component.InvokeAsync(async () => await component.Instance.Move(999, isUp: false));

        //Assert
        var state = component.Instance.Participants;
        state.Should().BeEmpty();
    }

    [Test]
    public async Task MoveDown_ItemNotInList_ShouldDoNothing()
    {
        //Arrange
        var inList = CreateItem(100, "InList", 5);
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<EncounterParticipant>>([inList]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));

        //Act
        await component.InvokeAsync(async () => await component.Instance.Move(999, isUp: false));

        //Assert
        var state = component.Instance.Participants;
        state.Should().ContainSingle(x => x.Name == "InList");
    }

    [Test]
    public async Task MoveDown_LastItem_ShouldStayAtBottom()
    {
        //Arrange
        var first = CreateItem(100, "First", 20);
        var second = CreateItem(101, "Second", 15);
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<EncounterParticipant>>([first, second]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));

        //Act
        await component.InvokeAsync(async () => await component.Instance.Move(second.Id, isUp: false));

        //Assert
        var list = component.Instance.Participants;
        list[0].Name.Should().Be("First");
        list[1].Name.Should().Be("Second");
    }

    [Test]
    public async Task MoveDown_OnePosition_ShouldSwapWithNext()
    {
        //Arrange
        var first = CreateItem(100, "First", 20);
        var second = CreateItem(101, "Second", 15);
        var third = CreateItem(102, "Third", 10);
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<EncounterParticipant>>([first, second, third]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));

        //Act
        await component.InvokeAsync(async () => await component.Instance.Move(second.Id, isUp: false));

        //Assert
        var list = component.Instance.Participants;
        list[0].Name.Should().Be("First");
        list[1].Name.Should().Be("Third");
        list[2].Name.Should().Be("Second");
    }

    [Test]
    public async Task MoveDown_FirstItem_ToBottom_MultipleMoves()
    {
        //Arrange
        var a = CreateItem(100, "A", 10);
        var b = CreateItem(101, "B", 20);
        var c = CreateItem(102, "C", 30);
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<EncounterParticipant>>([a, b, c]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));

        //Act
        for (int i = 0; i < 5; i++)
        {
            await component.InvokeAsync(async () => await component.Instance.Move(a.Id, isUp: false));
        }

        //Assert
        var list = component.Instance.Participants;
        list[^1].Name.Should().Be("A");
    }

    [Test]
    public async Task MoveUp_SingleItem_ShouldDoNothing()
    {
        //Arrange
        var item = CreateItem(100, "Solo", 5);
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<EncounterParticipant>>([item]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));

        //Act
        await component.InvokeAsync(async () => await component.Instance.Move(item.Id, isUp: true));

        //Assert
        var state = component.Instance.Participants;
        state.Should().ContainSingle(x => x.Name == "Solo");
    }

    [Test]
    public async Task MoveDown_SingleItem_ShouldDoNothing()
    {
        //Arrange
        var item = CreateItem(100, "Solo", 5);
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<EncounterParticipant>>([item]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));

        //Act
        await component.InvokeAsync(async () => await component.Instance.Move(item.Id, isUp: false));

        //Assert
        var state = component.Instance.Participants;
        state.Should().ContainSingle(x => x.Name == "Solo");
    }

    [Test]
    public async Task MoveUp_MultipleItems_ComplexReordering()
    {
        //Arrange
        var a = CreateItem(100, "A", 10);
        var b = CreateItem(101, "B", 20);
        var c = CreateItem(102, "C", 30);
        var d = CreateItem(103, "D", 40);
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<EncounterParticipant>>([a, b, c, d]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));

        //Act + Assert step 1
        await component.InvokeAsync(async () => await component.Instance.Move(d.Id, isUp: true));
        var list = component.Instance.Participants;
        list[0].Name.Should().Be("A");
        list[1].Name.Should().Be("B");
        list[2].Name.Should().Be("D");
        list[3].Name.Should().Be("C");

        //Act + Assert step 2
        await component.InvokeAsync(async () => await component.Instance.Move(d.Id, isUp: true));
        list = component.Instance.Participants;
        list[0].Name.Should().Be("A");
        list[1].Name.Should().Be("D");
        list[2].Name.Should().Be("B");
        list[3].Name.Should().Be("C");
    }

    [Test]
    public async Task MoveDown_MultipleItems_ComplexReordering()
    {
        //Arrange
        var a = CreateItem(100, "A", 10);
        var b = CreateItem(101, "B", 20);
        var c = CreateItem(102, "C", 30);
        var d = CreateItem(103, "D", 40);
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<EncounterParticipant>>([a, b, c, d]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));

        //Act + Assert step 1
        await component.InvokeAsync(async () => await component.Instance.Move(a.Id, isUp: false));
        var list = component.Instance.Participants;
        list[0].Name.Should().Be("B");
        list[1].Name.Should().Be("A");
        list[2].Name.Should().Be("C");
        list[3].Name.Should().Be("D");

        //Act + Assert step 2
        await component.InvokeAsync(async () => await component.Instance.Move(a.Id, isUp: false));
        list = component.Instance.Participants;
        list[0].Name.Should().Be("B");
        list[1].Name.Should().Be("C");
        list[2].Name.Should().Be("A");
        list[3].Name.Should().Be("D");
    }

    [Test]
    public async Task Move_NoEncounterSelected_ShouldDoNothing()
    {
        //Arrange
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<EncounterParticipant>>([]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();

        //Act
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(null!));
        await component.InvokeAsync(async () => await component.Instance.Move(999, isUp: true));

        //Assert
        await _participantsRepository.DidNotReceiveWithAnyArgs().SetEncounterParticipantsAsync(default, default!);
    }

    [Test]
    public async Task Move_ShouldPersistToRepository()
    {
        //Arrange
        var a = CreateItem(100, "A", 10);
        var b = CreateItem(101, "B", 20);
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<EncounterParticipant>>([a, b]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));

        //Act
        await component.InvokeAsync(async () => await component.Instance.Move(a.Id, isUp: false));

        //Assert
        await _participantsRepository.Received(1).SetEncounterParticipantsAsync(_encounter.Id, Arg.Any<List<EncounterParticipant>>());
    }

    static EncounterParticipant CreateItem(int id, string name, int initiative) => new()
    {
        Id = id,
        Name = name,
        Initiative = initiative,
        HitsAverage = 10,
        HitsDefault = 10,
        HitsCurrent = 10,
        ArmorClass = 12,
        ArmorClassCurrent = 12,
        Source = Source.Manual
    };
}