using Bunit;
using FluentAssertions;
using InitiativeTracker.DataAccess.Repositories;
using InitiativeTracker.Domain.Entities;
using InitiativeTracker.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace InitiativeTracker.Tests.HandlerTests;

public class InitiativeServiceSortTests
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
    public async Task SortByInitiative_EmptyList_ShouldStayEmpty()
    {
        //Arrange
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<EncounterParticipant>>([]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();

        //Act
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));
        await component.InvokeAsync(async () => await component.Instance.SortByInitiative());

        //Assert
        component.Instance.Participants.Should().BeEmpty();
    }

    [Test]
    public async Task SortByInitiative_SingleItem_ShouldNotChange()
    {
        //Arrange
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<EncounterParticipant>>([CreateItem(100, "Solo", 5)]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();

        //Act
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));
        await component.InvokeAsync(async () => await component.Instance.SortByInitiative());

        //Assert
        component.Instance.Participants.Should().ContainSingle(x => x.Name == "Solo");
    }

    [Test]
    public async Task SortByInitiative_TwoItems_ShouldSortDescending()
    {
        //Arrange
        var low = CreateItem(100, "Low", 5);
        var high = CreateItem(101, "High", 20);
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<EncounterParticipant>>([low, high]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult(new[]{low, high}.OrderByDescending(i => i.Initiative).ToList()));

        //Act
        await component.InvokeAsync(async () => await component.Instance.SortByInitiative());

        //Assert
        var list = component.Instance.Participants;
        list[0].Name.Should().Be("High");
        list[1].Name.Should().Be("Low");
    }

    [Test]
    public async Task SortByInitiative_MultipleItems_ShouldSortDescending()
    {
        //Arrange
        var a = CreateItem(100, "A", 10);
        var b = CreateItem(101, "B", 25);
        var c = CreateItem(102, "C", 15);
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<EncounterParticipant>>([a, b, c]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult(new[]{a, b, c}.OrderByDescending(i => i.Initiative).ToList()));

        //Act
        await component.InvokeAsync(async () => await component.Instance.SortByInitiative());

        //Assert
        var list = component.Instance.Participants;
        list[0].Name.Should().Be("B");
        list[1].Name.Should().Be("C");
        list[2].Name.Should().Be("A");
    }

    [Test]
    public async Task SortByInitiative_AlreadySorted_ShouldKeepOrder()
    {
        //Arrange
        var high = CreateItem(100, "High", 30);
        var mid = CreateItem(101, "Mid", 20);
        var low = CreateItem(102, "Low", 10);
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<EncounterParticipant>>([high, mid, low]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();

        //Act
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));
        await component.InvokeAsync(async () => await component.Instance.SortByInitiative());

        //Assert
        var list = component.Instance.Participants;
        list[0].Name.Should().Be("High");
        list[1].Name.Should().Be("Mid");
        list[2].Name.Should().Be("Low");
    }

    [Test]
    public async Task SortByInitiative_SameInitiative_ShouldKeepOrder()
    {
        //Arrange
        var first = CreateItem(100, "First", 15);
        var second = CreateItem(101, "Second", 15);
        var third = CreateItem(102, "Third", 15);
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<EncounterParticipant>>([first, second, third]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();

        //Act
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));
        await component.InvokeAsync(async () => await component.Instance.SortByInitiative());

        //Assert
        var list = component.Instance.Participants;
        list.Should().HaveCount(3);
        list[0].Initiative.Should().Be(15);
        list[1].Initiative.Should().Be(15);
        list[2].Initiative.Should().Be(15);
    }

    [Test]
    public async Task SortByInitiative_ShouldCallSaveParticipantAsync()
    {
        //Arrange
        var low = CreateItem(100, "Low", 5);
        var high = CreateItem(101, "High", 20);
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<EncounterParticipant>>([low, high]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();

        //Act
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));
        await component.InvokeAsync(async () => await component.Instance.SortByInitiative());

        //Assert
        await _participantsRepository.Received(1).SetEncounterParticipantsAsync(_encounter.Id, Arg.Any<List<EncounterParticipant>>());
    }

    [Test]
    public async Task SortByInitiative_NoEncounterSelected_ShouldNotSave()
    {
        //Arrange
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<EncounterParticipant>>([]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();

        //Act
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(null!));
        await component.InvokeAsync(async () => await component.Instance.SortByInitiative());

        //Assert
        await _participantsRepository.DidNotReceiveWithAnyArgs().SetEncounterParticipantsAsync(default, default!);
    }

    [Test]
    public async Task SortByInitiative_FullReverseOrder_ShouldFlipCompletely()
    {
        //Arrange
        var a = CreateItem(100, "A", 5);
        var b = CreateItem(101, "B", 10);
        var c = CreateItem(102, "C", 15);
        var d = CreateItem(103, "D", 20);
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult<List<EncounterParticipant>>([a, b, c, d]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));
        _participantsRepository.GetAllEncounterParticipantsAsync(Arg.Any<int>()).Returns(Task.FromResult(new[]{a, b, c, d}.OrderByDescending(i => i.Initiative).ToList()));

        //Act
        await component.InvokeAsync(async () => await component.Instance.SortByInitiative());

        //Assert
        var list = component.Instance.Participants;
        list[0].Name.Should().Be("D");
        list[1].Name.Should().Be("C");
        list[2].Name.Should().Be("B");
        list[3].Name.Should().Be("A");
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
