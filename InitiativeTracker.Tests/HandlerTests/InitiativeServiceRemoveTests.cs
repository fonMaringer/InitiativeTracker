using Bunit;
using FluentAssertions;
using InitiativeTracker.DataAccess.Repositories;
using InitiativeTracker.Domain.Entities;
using InitiativeTracker.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace InitiativeTracker.Tests.HandlerTests;

public class InitiativeServiceRemoveTests
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
    public async Task Remove_ShouldRemoveMatchingItem()
    {
        //Arrange
        var item1 = CreateItem(100, "Goblin", 15);
        var item2 = CreateItem(101, "Orc", 20);
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<EncounterParticipant>>([item1, item2]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        
        //Act
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));
        _encounter.CurrentActiveParticipantId = null;
        await component.InvokeAsync(async () => await component.Instance.Remove(item1.Id));
        
        //Assert
        var state = component.Instance.Participants;
        state.Should().ContainSingle();
        state[0].Name.Should().Be("Orc");
    }

    [Test]
    public async Task Remove_WhenItemIdNotInList_ShouldDoNothing()
    {
        //Arrange
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<EncounterParticipant>>([CreateItem(100, "Goblin", 15)]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        
        //Act
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));
        _encounter.CurrentActiveParticipantId = null;
        await component.InvokeAsync(async () => await component.Instance.Remove(999));
        
        //Assert
        var state = component.Instance.Participants;
        state.Should().ContainSingle();
        state[0].Name.Should().Be("Goblin");
    }

    [Test]
    public async Task Remove_LastItem_ShouldWork()
    {
        //Arrange
        var item1 = CreateItem(100, "First", 10);
        var item2 = CreateItem(101, "Second", 20);
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<EncounterParticipant>>([item1, item2]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        
        //Act
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));
        _encounter.CurrentActiveParticipantId = null;
        await component.InvokeAsync(async () => await component.Instance.Remove(item2.Id));
        
        //Assert
        var state = component.Instance.Participants;
        state.Should().ContainSingle();
        state[0].Name.Should().Be("First");
    }

    [Test]
    public async Task Remove_MiddleItem_ShouldReorder()
    {
        //Arrange
        var item1 = CreateItem(100, "A", 5);
        var item2 = CreateItem(101, "B", 10);
        var item3 = CreateItem(102, "C", 15);
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<EncounterParticipant>>([item1, item2, item3]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        
        //Act
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));
        _encounter.CurrentActiveParticipantId = null;
        await component.InvokeAsync(async () => await component.Instance.Remove(item2.Id));
        
        //Assert
        var state = component.Instance.Participants;
        state[0].Name.Should().Be("A");
        state[1].Name.Should().Be("C");
    }

    [Test]
    public async Task Remove_SingleItem_ShouldEmptyList()
    {
        //Arrange
        var item = CreateItem(100, "Solo", 10);
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<EncounterParticipant>>([item]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        
        //Act
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));
        _encounter.CurrentActiveParticipantId = null;
        await component.InvokeAsync(async () => await component.Instance.Remove(item.Id));
        
        //Assert
        var state = component.Instance.Participants;
        state.Should().BeEmpty();
    }

    [Test]
    public async Task Remove_CurrentItem_ShouldUpdateEncounterState()
    {
        //Arrange
        var item1 = CreateItem(100, "First", 10);
        var item2 = CreateItem(101, "Second", 20);
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<EncounterParticipant>>([item1, item2]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        
        //Act
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));
        _encounter.CurrentActiveParticipantId = item1.Id;
        await component.InvokeAsync(async () => await component.Instance.Remove(item1.Id));
        
        //Assert
        await _encounterRepository.Received(1).UpdateEncounterAsync(Arg.Any<InitiativeTracker.DataAccess.Dtos.EncounterUpdateDto>());
    }

    [Test]
    public async Task Clear_ShouldEmptyListAndResetState()
    {
        //Arrange
        var item1 = CreateItem(100, "A", 5);
        var item2 = CreateItem(101, "B", 10);
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<EncounterParticipant>>([item1, item2]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        
        //Act
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));
        await component.InvokeAsync(async () => await component.Instance.Clear());
        
        //Assert
        var state = component.Instance.Participants;
        state.Should().BeEmpty();
    }

    [Test]
    public async Task Remove_NoEncounterSelected_ShouldDoNothing()
    {
        //Arrange
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        
        //Act
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(null!));
        await component.InvokeAsync(async () => await component.Instance.Remove(100));
        
        //Assert
        await _participantsRepository.DidNotReceiveWithAnyArgs().SetEncounterParticipantsAsync(default, default!);
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
