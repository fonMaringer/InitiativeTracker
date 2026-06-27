using Bunit;
using FluentAssertions;
using InitiativeTracker.DataAccess.Repositories;
using InitiativeTracker.Domain.Entities;
using InitiativeTracker.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace InitiativeTracker.Tests.HandlerTests;

public class InitiativeServiceAppendTests
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
    public async Task Participants_Initially_ShouldBeEmpty()
    {
        //Arrange
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<EncounterParticipant>>([]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        
        //Act
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));
        
        //Assert
        var state = component.Instance.Participants;
        state.Should().BeEmpty();
    }

    [Test]
    public async Task AddToEncounter_WhenEmpty_ShouldAddSingleItem()
    {
        //Arrange
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<EncounterParticipant>>([]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        
        //Act
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));
        var item = CreateItem("Goblin", 15);
        await component.InvokeAsync(async () => await component.Instance.AddToEncounter(item));
        
        //Assert
        var state = component.Instance.Participants;
        state.Should().ContainSingle();
        state[0].Name.Should().Be("Goblin");
        state[0].Initiative.Should().Be(15);
    }

    [Test]
    public async Task AddToEncounter_MultipleItems_ShouldAddAll()
    {
        //Arrange
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<EncounterParticipant>>([]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        
        //Act
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));
        await component.InvokeAsync(async () => await component.Instance.AddToEncounter(CreateItem("Goblin", 15)));
        await component.InvokeAsync(async () => await component.Instance.AddToEncounter(CreateItem("Orc", 20)));
        await component.InvokeAsync(async () => await component.Instance.AddToEncounter(CreateItem("Troll", 25)));
        
        //Assert
        var state = component.Instance.Participants;
        state.Should().HaveCount(3);
    }

    [Test]
    public async Task AddToEncounter_PreservesInsertionOrder()
    {
        //Arrange
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<EncounterParticipant>>([]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        
        //Act
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));
        var itemA = CreateItem("First", 10);
        var itemB = CreateItem("Second", 20);
        await component.InvokeAsync(async () => await component.Instance.AddToEncounter(itemA));
        await component.InvokeAsync(async () => await component.Instance.AddToEncounter(itemB));
        
        //Assert
        var state = component.Instance.Participants;
        state[0].Name.Should().Be("First");
        state[1].Name.Should().Be("Second");
    }

    [Test]
    public async Task AddManual_WithName_ShouldCreateAndAddParticipant()
    {
        //Arrange
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<EncounterParticipant>>([]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        
        //Act
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));
        await component.InvokeAsync(async () => await component.Instance.AddManual());
        
        //Assert
        var setCall = _participantsRepository.ReceivedCalls().FirstOrDefault(c => c.GetMethodInfo()?.Name == nameof(IEncounterParticipantsRepository.SetEncounterParticipantsAsync));
        setCall.Should().NotBeNull("SetEncounterParticipantsAsync should have been called after AddManual sets form fields");
    }

    [Test]
    public async Task AddToEncounter_NoEncounterSelected_ShouldNotAdd()
    {
        //Arrange
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<EncounterParticipant>>([]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        
        //Act
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(null!));
        var item = CreateItem("Goblin", 15);
        await component.InvokeAsync(async () => await component.Instance.AddToEncounter(item));
        
        //Assert
        await _participantsRepository.DidNotReceiveWithAnyArgs().SetEncounterParticipantsAsync(default, default!);
    }

    [Test]
    public async Task AddMultiple_WithLoadedEncounter_ShouldCallSave()
    {
        //Arrange
        var existing = new List<EncounterParticipant> { CreateItem("Existing", 10) };
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult(existing));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        
        //Act
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));
        await component.InvokeAsync(async () => await component.Instance.AddToEncounter(CreateItem("New", 20)));
        
        //Assert
        await _participantsRepository.Received(1).SetEncounterParticipantsAsync(_encounter.Id, Arg.Any<List<EncounterParticipant>>());
    }

    static EncounterParticipant CreateItem(string name, int initiative) => new()
    {
        Id = Random.Shared.Next(1000),
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
