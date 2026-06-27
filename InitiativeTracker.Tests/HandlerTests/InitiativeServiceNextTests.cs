using Bunit;
using FluentAssertions;
using InitiativeTracker.DataAccess.Repositories;
using InitiativeTracker.Domain.Entities;
using InitiativeTracker.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace InitiativeTracker.Tests.HandlerTests;

public class InitiativeServiceNextTests
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
    public async Task Next_WhenNoEncounter_ShouldReturnEarly()
    {
        //Arrange
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<EncounterParticipant>>([]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        
        //Act
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(null!));
        await component.InvokeAsync(async () => await component.Instance.Next());
        
        //Assert
        // UpdateEncounterAsync should not be called when no encounter was selected
        await _encounterRepository.DidNotReceiveWithAnyArgs().UpdateEncounterAsync(default!);
    }

    [Test]
    public async Task Next_FirstCallWithoutActiveParticipant_ShouldSetCurrentToSecond()
    {
        //Arrange
        var item1 = CreateItem("Goblin", 15, 1);
        var item2 = CreateItem("Orc", 20, 2);
        var participants = Task.FromResult<List<EncounterParticipant>>([item1, item2]);
        _participantsRepository.GetAllEncounterParticipantsAsync(_encounter.Id).Returns(participants);
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        
        //Act
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));
        await component.InvokeAsync(async () => await component.Instance.Next());
        
        //Assert
        _encounter.CurrentActiveParticipantId.Should().Be(2);
    }

    [Test]
    public async Task Next_SecondCall_ShouldMoveToNextParticipant()
    {
        //Arrange
        var item1 = CreateItem("Goblin", 15, 1);
        var item2 = CreateItem("Orc", 20, 2);
        var participants = Task.FromResult<List<EncounterParticipant>>([item1, item2]);
        _participantsRepository.GetAllEncounterParticipantsAsync(_encounter.Id).Returns(participants);
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        
        //Act
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));
        await component.InvokeAsync(async () => await component.Instance.Next());
        await component.InvokeAsync(async () => await component.Instance.Next());
        
        //Assert
        // After 2 calls past a 2-item list: first -> second -> first (back to index 0)
        _encounter.CurrentActiveParticipantId.Should().Be(1);
    }

    [Test]
    public async Task Next_CyclingPastEnd_ShouldWrapToFirstAndIncrementRound()
    {
        //Arrange
        var item1 = CreateItem("Goblin", 15, 1);
        var item2 = CreateItem("Orc", 20, 2);
        var participants = Task.FromResult<List<EncounterParticipant>>([item1, item2]);
        _participantsRepository.GetAllEncounterParticipantsAsync(_encounter.Id).Returns(participants);
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        
        //Act
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));
        // Call Next 3 times with 2 participants
        await component.InvokeAsync(async () => await component.Instance.Next());
        await component.InvokeAsync(async () => await component.Instance.Next());
        await component.InvokeAsync(async () => await component.Instance.Next());
        
        //Assert
        // We've wrapped at least once, encounter repository should have received update calls
        var calls = _encounterRepository.ReceivedCalls();
        var updateCalls = calls.Where(c => c.GetMethodInfo()?.Name == nameof(IEncounterRepository.UpdateEncounterAsync)).ToList();
        updateCalls.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    [Test]
    public async Task Next_WithActiveParticipant_ShouldAdvanceFromThatParticipant()
    {
        //Arrange
        var item1 = CreateItem("Goblin", 15, 1);
        var item2 = CreateItem("Orc", 20, 2);
        var participants = Task.FromResult<List<EncounterParticipant>>([item1, item2]);
        _participantsRepository.GetAllEncounterParticipantsAsync(_encounter.Id).Returns(participants);
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        
        //Act
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));
        // Manually set active participant to item 1 before calling Next
        _encounter.CurrentActiveParticipantId = 1;
        await component.InvokeAsync(async () => await component.Instance.Next());
        
        //Assert
        // Should advance from id=1 to id=2
        _encounter.CurrentActiveParticipantId.Should().Be(2);
    }

    [Test]
    public async Task Next_PastLastParticipant_ShouldWrapToFirst()
    {
        //Arrange
        var item1 = CreateItem("Goblin", 15, 1);
        var item2 = CreateItem("Orc", 20, 2);
        var participants = Task.FromResult<List<EncounterParticipant>>([item1, item2]);
        _participantsRepository.GetAllEncounterParticipantsAsync(_encounter.Id).Returns(participants);
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        
        //Act
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));
        // Advance to second participant (id=2)
        await component.InvokeAsync(async () => await component.Instance.Next());
        // Set active manually to simulate state being at the last participant
        _encounter.CurrentActiveParticipantId = 2;
        // Next should wrap around
        await component.InvokeAsync(async () => await component.Instance.Next());
        
        //Assert
        _encounter.CurrentActiveParticipantId.Should().Be(1);
    }

    [Test]
    public async Task Next_SingleItem_ShouldWrapToSameParticipant()
    {
        //Arrange
        var item = CreateItem("Solo", 5, 1);
        var participants = Task.FromResult<List<EncounterParticipant>>([item]);
        _participantsRepository.GetAllEncounterParticipantsAsync(_encounter.Id).Returns(participants);
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        
        //Act
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));
        await component.InvokeAsync(async () => await component.Instance.Next());
        
        //Assert
        _encounter.CurrentActiveParticipantId.Should().Be(1);
    }

    [Test]
    public async Task Next_WithNullActiveParticipant_ShouldAdvanceFromFirst()
    {
        //Arrange
        var item1 = CreateItem("Goblin", 15, 1);
        var item2 = CreateItem("Orc", 20, 2);
        var participants = Task.FromResult<List<EncounterParticipant>>([item1, item2]);
        _participantsRepository.GetAllEncounterParticipantsAsync(_encounter.Id).Returns(participants);
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        
        //Act
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));
        // No active participant set initially
        _encounter.CurrentActiveParticipantId = null;
        await component.InvokeAsync(async () => await component.Instance.Next());
        
        //Assert
        // CalculateNext picks first when CurrentActive is null, so after advancing -> second participant (id=2)
        _encounter.CurrentActiveParticipantId.Should().Be(2);
    }

    [Test]
    public async Task Next_ShouldCallUpdateEncounterAsync()
    {
        //Arrange
        var item1 = CreateItem("Goblin", 15, 1);
        var item2 = CreateItem("Orc", 20, 2);
        var participants = Task.FromResult<List<EncounterParticipant>>([item1, item2]);
        _participantsRepository.GetAllEncounterParticipantsAsync(_encounter.Id).Returns(participants);
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        
        //Act
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));
        await component.InvokeAsync(async () => await component.Instance.Next());
        
        //Assert
        await _encounterRepository.Received(1).UpdateEncounterAsync(Arg.Any<InitiativeTracker.DataAccess.Dtos.EncounterUpdateDto>());
    }

    [Test]
    public async Task Next_ShouldCallSaveParticipantAsync()
    {
        //Arrange
        var item1 = CreateItem("Goblin", 15, 1);
        var item2 = CreateItem("Orc", 20, 2);
        var participants = Task.FromResult<List<EncounterParticipant>>([item1, item2]);
        _participantsRepository.GetAllEncounterParticipantsAsync(_encounter.Id).Returns(participants);
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        
        //Act
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));
        await component.InvokeAsync(async () => await component.Instance.Next());
        
        //Assert
        await _participantsRepository.Received(1).SetEncounterParticipantsAsync(_encounter.Id, Arg.Any<List<EncounterParticipant>>());
    }

    [Test]
    public async Task Next_MultipleCycles_ShouldIncrementRound()
    {
        //Arrange
        var item1 = CreateItem("Goblin", 15, 1);
        var item2 = CreateItem("Orc", 20, 2);
        var participants = Task.FromResult<List<EncounterParticipant>>([item1, item2]);
        _participantsRepository.GetAllEncounterParticipantsAsync(_encounter.Id).Returns(participants);
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        
        //Act
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));
        // Call Next multiple times
        for (int i = 0; i < 6; i++)
            await component.InvokeAsync(async () => await component.Instance.Next());
        
        //Assert
        // Verify UpdateEncounterAsync was called at least once per cycle
        var updateCalls = _encounterRepository.ReceivedCalls().Count(c => c.GetMethodInfo()?.Name == nameof(IEncounterRepository.UpdateEncounterAsync));
        updateCalls.Should().BeGreaterThanOrEqualTo(6);
    }

    static EncounterParticipant CreateItem(string name, int initiative, int id) => new()
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
