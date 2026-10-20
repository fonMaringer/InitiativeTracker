using Bunit;
using FluentAssertions;
using InitiativeTracker.DataAccess.Repositories;
using InitiativeTracker.Domain.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using NSubstitute;

namespace InitiativeTracker.Tests.HandlerTests;

public class EncounterCatalogCopyTests
{
    private BunitContext _ctx;

    private IEncounterRepository _encounterRepository;
    private IJSRuntime _jsRuntime;
    private Encounter _encounter;
    private Encounter _copy;
    private List<Encounter?> _received;

    [SetUp]
    public void Setup()
    {
        _ctx = new BunitContext();

        _encounterRepository = Substitute.For<IEncounterRepository>();
        _jsRuntime = Substitute.For<IJSRuntime>();
        _ctx.Services.AddSingleton(_encounterRepository);
        _ctx.Services.AddSingleton(_jsRuntime);

        _received = [];
        _encounter = new Encounter { Id = 1, Name = "Boss Fight", CreatedAt = DateTime.UtcNow };
        _copy = new Encounter { Id = 2, Name = "Boss Fight (copy)", CreatedAt = DateTime.UtcNow };

        _encounterRepository
            .GetAllEncountersAsync()
            .Returns(Task.FromResult<List<Encounter>>([_encounter]));
    }

    [TearDown]
    public void TearDown()
    {
        _ctx.Dispose();
    }

    private IRenderedComponent<Components.Pages.Encounters.EncounterCatalog> RenderCatalog()
    {
        return _ctx.Render<Components.Pages.Encounters.EncounterCatalog>(parameters =>
            parameters.Add(
                p => p.OnSelectEncounter,
                EventCallback.Factory.Create<Encounter?>(this, value => _received.Add(value))));
    }

    [Test]
    public async Task CopyEncounter_ShouldCallRepositoryWithSourceIdAndNameSuffix()
    {
        //Arrange
        _encounterRepository
            .CopyEncounterAsync(Arg.Any<int>(), Arg.Any<string>())
            .Returns(Task.FromResult<Encounter?>(_copy));
        var component = RenderCatalog();

        //Act
        await component.InvokeAsync(async () => await component.Instance.CopyEncounter(_encounter));

        //Assert
        await _encounterRepository.Received(1).CopyEncounterAsync(1, "Boss Fight (copy)");
    }

    [Test]
    public async Task CopyEncounter_ShouldAppendCopyAndSelectIt()
    {
        //Arrange
        _encounterRepository
            .CopyEncounterAsync(Arg.Any<int>(), Arg.Any<string>())
            .Returns(Task.FromResult<Encounter?>(_copy));
        var component = RenderCatalog();

        //Act
        await component.InvokeAsync(async () => await component.Instance.CopyEncounter(_encounter));

        //Assert
        component.Instance.SelectedEncounter.Should().Be(_copy);
        component.Markup.Should().Contain("Boss Fight (copy)");
        _received.Should().ContainSingle().Which.Should().Be(_copy);
    }

    [Test]
    public async Task CopyEncounter_WhenSourceNotFound_ShouldNotChangeSelection()
    {
        //Arrange
        _encounterRepository
            .CopyEncounterAsync(Arg.Any<int>(), Arg.Any<string>())
            .Returns(Task.FromResult<Encounter?>(null));
        var component = RenderCatalog();

        //Act
        await component.InvokeAsync(async () => await component.Instance.CopyEncounter(_encounter));

        //Assert
        component.Instance.SelectedEncounter.Should().BeNull();
        _received.Should().BeEmpty();
    }
}
