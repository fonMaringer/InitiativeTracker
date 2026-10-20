using Bunit;
using FluentAssertions;
using InitiativeTracker.DataAccess.Dtos;
using InitiativeTracker.DataAccess.Repositories;
using InitiativeTracker.Domain.Entities;
using InitiativeTracker.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace InitiativeTracker.Tests.HandlerTests;

public class InitiativeServiceSkipDeadTests
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
        // SkipDead defaults to true, which is the default-on expectation
        _encounter = new Encounter { Id = 1, Name = "Boss Fight", CurrentRound = 1 };
    }

    [TearDown]
    public void TearDown()
    {
        _ctx.Dispose();
    }

    private IRenderedComponent<Components.Pages.Encounters.InitiativeTracker> RenderWith(List<EncounterParticipant> participants)
    {
        _participantsRepository
            .GetAllEncounterParticipantsAsync(_encounter.Id)
            .Returns(Task.FromResult(participants));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        return component;
    }

    [Test]
    public async Task Next_SkipEnabled_DeadNextParticipant_ShouldAdvancePastIt()
    {
        //Arrange
        var alive1 = CreateItem("Goblin", 15, 1);
        var dead = CreateDeadItem("Orc", 12, 2);
        var alive2 = CreateItem("Troll", 5, 3);
        _encounter.CurrentActiveParticipantId = 1;
        var component = RenderWith([alive1, dead, alive2]);
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));

        //Act
        await component.InvokeAsync(async () => await component.Instance.Next());

        //Assert
        _encounter.CurrentActiveParticipantId.Should().Be(3);
        _encounter.CurrentRound.Should().Be(1);
    }

    [Test]
    public async Task Next_SkipEnabled_DeadTail_ShouldWrapAndIncrementRoundOnce()
    {
        //Arrange
        var alive1 = CreateItem("Goblin", 15, 1);
        var alive2 = CreateItem("Orc", 12, 2);
        var dead = CreateDeadItem("Troll", 5, 3);
        _encounter.CurrentActiveParticipantId = 2;
        var component = RenderWith([alive1, alive2, dead]);
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));

        //Act
        await component.InvokeAsync(async () => await component.Instance.Next());

        //Assert
        _encounter.CurrentActiveParticipantId.Should().Be(1);
        _encounter.CurrentRound.Should().Be(2);
    }

    [Test]
    public async Task Next_SkipEnabled_AllDead_ShouldFallBackToNormalAdvance()
    {
        //Arrange
        var dead1 = CreateDeadItem("Goblin", 15, 1);
        var dead2 = CreateDeadItem("Orc", 12, 2);
        var dead3 = CreateDeadItem("Troll", 5, 3);
        _encounter.CurrentActiveParticipantId = 3;
        var component = RenderWith([dead1, dead2, dead3]);
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));

        //Act
        await component.InvokeAsync(async () => await component.Instance.Next());

        //Assert
        // Wrap around to first participant and increment the round once
        _encounter.CurrentActiveParticipantId.Should().Be(1);
        _encounter.CurrentRound.Should().Be(2);
    }

    [Test]
    public async Task Next_SkipDisabled_DeadNextParticipant_ShouldPickIt()
    {
        //Arrange
        _encounter.SkipDead = false;
        var alive1 = CreateItem("Goblin", 15, 1);
        var dead = CreateDeadItem("Orc", 12, 2);
        var alive2 = CreateItem("Troll", 5, 3);
        _encounter.CurrentActiveParticipantId = 1;
        var component = RenderWith([alive1, dead, alive2]);
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));

        //Act
        await component.InvokeAsync(async () => await component.Instance.Next());

        //Assert
        _encounter.CurrentActiveParticipantId.Should().Be(2);
        _encounter.CurrentRound.Should().Be(1);
    }

    [Test]
    public async Task Next_SkipEnabled_NullActive_ShouldPickFirstLivingParticipant()
    {
        //Arrange
        var dead = CreateDeadItem("Goblin", 15, 1);
        var alive = CreateItem("Orc", 12, 2);
        var component = RenderWith([dead, alive]);
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));

        //Act
        await component.InvokeAsync(async () => await component.Instance.Next());

        //Assert
        _encounter.CurrentActiveParticipantId.Should().Be(2);
    }

    [Test]
    public async Task Next_SkipDisabled_NullActive_ShouldPickFirstParticipant()
    {
        //Arrange
        _encounter.SkipDead = false;
        var dead = CreateDeadItem("Goblin", 15, 1);
        var alive = CreateItem("Orc", 12, 2);
        var component = RenderWith([dead, alive]);
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));

        //Act
        await component.InvokeAsync(async () => await component.Instance.Next());

        //Assert
        _encounter.CurrentActiveParticipantId.Should().Be(1);
    }

    [Test]
    public async Task SaveSkipDead_ShouldPersistToggleValue()
    {
        //Arrange
        _encounter.SkipDead = false;
        _participantsRepository
            .GetAllEncounterParticipantsAsync(Arg.Any<int>())
            .Returns(Task.FromResult<List<EncounterParticipant>>([]));
        var component = _ctx.Render<Components.Pages.Encounters.InitiativeTracker>();
        await component.InvokeAsync(async () => await component.Instance.SelectEncounterAsync(_encounter));

        //Act
        await component.InvokeAsync(async () => await component.Instance.SaveSkipDead());

        //Assert
        await _encounterRepository.Received(1).UpdateEncounterAsync(
            Arg.Is<EncounterUpdateDto>(dto => dto.Id == 1 && dto.SkipDead == false));
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

    static EncounterParticipant CreateDeadItem(string name, int initiative, int id)
    {
        var item = CreateItem(name, initiative, id);
        item.HitsCurrent = 0;
        return item;
    }
}
