using FluentAssertions;
using InitiativeTracker.Application;
using InitiativeTracker.Application.Dtos;
using InitiativeTracker.Domain;
using InitiativeTracker.Domain.Entities;
using InitiativeTracker.Domain.Enums;
using InitiativeTracker.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace InitiativeTracker.Tests;

public class SpellServiceTests
{
    private ILogger<SpellService> _logger;
    private InitiativeTrackerDbContext _dbContext;
    private SpellService _service;

    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<SpellService>>();
        var options = new DbContextOptionsBuilder<InitiativeTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"SpellDb-{Guid.NewGuid()}")
            .Options;
        _dbContext = new InitiativeTrackerDbContext(options);
        _service = new SpellService(_logger, _dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    // AddAsync tests

    [Test]
    public async Task AddAsync_ValidDto_ShouldAddSpell()
    {
        var dto = CreateSpellCreateDto("Fireball", true, true, false, SpellClass.Wizard, "A bright streak flashes.");

        await _service.AddAsync(dto);

        var result = await _dbContext.Spells.ToListAsync();
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Fireball");
        result[0].VerbalComponent.Should().BeTrue();
        result[0].SomaticComponent.Should().BeTrue();
        result[0].MaterialComponent.Should().BeFalse();
        result[0].Class.Should().Be(SpellClass.Wizard);
        result[0].Description.Should().Be("A bright streak flashes.");
    }

    [Test]
    public async Task AddAsync_AllComponentsTrue_ShouldPersistAll()
    {
        var dto = CreateSpellCreateDto("Lightning Bolt", true, true, true, SpellClass.Wizard, "A stroke of lightning.");

        await _service.AddAsync(dto);

        var result = await _dbContext.Spells.ToListAsync();
        result.Should().HaveCount(1);
        result[0].VerbalComponent.Should().BeTrue();
        result[0].SomaticComponent.Should().BeTrue();
        result[0].MaterialComponent.Should().BeTrue();
    }

    [Test]
    public async Task AddAsync_NoComponents_ShouldPersist()
    {
        var dto = CreateSpellCreateDto("Shield", false, false, false, SpellClass.Wizard, "An invisible barrier.");

        await _service.AddAsync(dto);

        var result = await _dbContext.Spells.ToListAsync();
        result.Should().HaveCount(1);
        result[0].VerbalComponent.Should().BeFalse();
        result[0].SomaticComponent.Should().BeFalse();
        result[0].MaterialComponent.Should().BeFalse();
    }

    [Test]
    public async Task AddAsync_MultipleSpells_ShouldPersistAll()
    {
        await _service.AddAsync(CreateSpellCreateDto("Fireball", true, true, false, SpellClass.Wizard, "Fire."));
        await _service.AddAsync(CreateSpellCreateDto("Cure Wounds", true, false, false, SpellClass.Cleric, "Healing."));

        var result = await _dbContext.Spells.ToListAsync();
        result.Should().HaveCount(2);
    }

    [Test]
    public async Task AddAsync_ClericClass_ShouldPersist()
    {
        var dto = CreateSpellCreateDto("Heal", true, false, true, SpellClass.Cleric, "Restores 70 HP.");

        await _service.AddAsync(dto);

        var result = await _dbContext.Spells.ToListAsync();
        result.Should().HaveCount(1);
        result[0].Class.Should().Be(SpellClass.Cleric);
    }

    [Test]
    public async Task AddAsync_BarbarianClass_ShouldPersist()
    {
        var dto = CreateSpellCreateDto("Heroism", true, false, false, SpellClass.Barbarian, "Grants advantage.");

        await _service.AddAsync(dto);

        var result = await _dbContext.Spells.ToListAsync();
        result.Should().HaveCount(1);
        result[0].Class.Should().Be(SpellClass.Barbarian);
    }

    [Test]
    public async Task AddAsync_DruidClass_ShouldPersist()
    {
        var dto = CreateSpellCreateDto("Call Lightning", true, true, false, SpellClass.Druid, "Summons lightning.");

        await _service.AddAsync(dto);

        var result = await _dbContext.Spells.ToListAsync();
        result.Should().HaveCount(1);
        result[0].Class.Should().Be(SpellClass.Druid);
    }

    [Test]
    public async Task AddAsync_HtmlDescription_ShouldPersist()
    {
        var htmlDesc = "<h4>Effect</h4><p>You hurl a <strong>veritable wand</strong> of thin flame.</p>";
        var dto = CreateSpellCreateDto("Firebolt", true, true, false, SpellClass.Wizard, htmlDesc);

        await _service.AddAsync(dto);

        var result = await _dbContext.Spells.ToListAsync();
        result.Should().HaveCount(1);
        result[0].Description.Should().Be(htmlDesc);
    }

    // GetByIdAsync tests

    [Test]
    public async Task GetByIdAsync_ExistingId_ShouldReturnSpell()
    {
        var dto = CreateSpellCreateDto("Fireball", true, true, false, SpellClass.Wizard, "A bright streak flashes.");
        await _service.AddAsync(dto);

        var entity = (await _dbContext.Spells.ToListAsync())[0];
        var found = await _service.GetByIdAsync(entity.Id);

        found.Should().NotBeNull();
        found!.Name.Should().Be("Fireball");
        found.Class.Should().Be(SpellClass.Wizard);
    }

    [Test]
    public async Task GetByIdAsync_NonExistentId_ShouldReturnNull()
    {
        var found = await _service.GetByIdAsync(999);

        found.Should().BeNull();
    }

    // UpdateAsync tests

    [Test]
    public async Task UpdateAsync_ExistingName_ShouldUpdateName()
    {
        var dto = CreateSpellCreateDto("Fireball", true, true, false, SpellClass.Wizard, "Fire.");
        await _service.AddAsync(dto);

        var entity = (await _dbContext.Spells.ToListAsync())[0];

        await _service.UpdateAsync(entity.Id, new SpellUpdateDto("Greater Fireball", null, null, null, null, null));

        var updated = await _service.GetByIdAsync(entity.Id);
        updated!.Name.Should().Be("Greater Fireball");
    }

    [Test]
    public async Task UpdateAsync_NullName_ShouldNotChangeName()
    {
        var dto = CreateSpellCreateDto("Fireball", true, true, false, SpellClass.Wizard, "Fire.");
        await _service.AddAsync(dto);

        var entity = (await _dbContext.Spells.ToListAsync())[0];

        await _service.UpdateAsync(entity.Id, new SpellUpdateDto(null, null, null, null, null, null));

        var updated = await _service.GetByIdAsync(entity.Id);
        updated!.Name.Should().Be("Fireball");
    }

    [Test]
    public async Task UpdateAsync_EmptyName_ShouldNotChangeName()
    {
        var dto = CreateSpellCreateDto("Fireball", true, true, false, SpellClass.Wizard, "Fire.");
        await _service.AddAsync(dto);

        var entity = (await _dbContext.Spells.ToListAsync())[0];

        await _service.UpdateAsync(entity.Id, new SpellUpdateDto("", null, null, null, null, null));

        var updated = await _service.GetByIdAsync(entity.Id);
        updated!.Name.Should().Be("Fireball");
    }

    [Test]
    public async Task UpdateAsync_WithVerbalComponent_ShouldUpdate()
    {
        var dto = CreateSpellCreateDto("Shield", false, false, false, SpellClass.Wizard, "Barrier.");
        await _service.AddAsync(dto);

        var entity = (await _dbContext.Spells.ToListAsync())[0];

        await _service.UpdateAsync(entity.Id, new SpellUpdateDto(null, true, null, null, null, null));

        var updated = await _service.GetByIdAsync(entity.Id);
        updated!.VerbalComponent.Should().BeTrue();
    }

    [Test]
    public async Task UpdateAsync_WithSomaticComponent_ShouldUpdate()
    {
        var dto = CreateSpellCreateDto("Shield", false, false, false, SpellClass.Wizard, "Barrier.");
        await _service.AddAsync(dto);

        var entity = (await _dbContext.Spells.ToListAsync())[0];

        await _service.UpdateAsync(entity.Id, new SpellUpdateDto(null, null, true, null, null, null));

        var updated = await _service.GetByIdAsync(entity.Id);
        updated!.SomaticComponent.Should().BeTrue();
    }

    [Test]
    public async Task UpdateAsync_WithMaterialComponent_ShouldUpdate()
    {
        var dto = CreateSpellCreateDto("Shield", false, false, false, SpellClass.Wizard, "Barrier.");
        await _service.AddAsync(dto);

        var entity = (await _dbContext.Spells.ToListAsync())[0];

        await _service.UpdateAsync(entity.Id, new SpellUpdateDto(null, null, null, true, null, null));

        var updated = await _service.GetByIdAsync(entity.Id);
        updated!.MaterialComponent.Should().BeTrue();
    }

    [Test]
    public async Task UpdateAsync_WithClass_ShouldUpdateClass()
    {
        var dto = CreateSpellCreateDto("Heroism", true, false, false, SpellClass.Paladin, "Courage.");
        await _service.AddAsync(dto);

        var entity = (await _dbContext.Spells.ToListAsync())[0];

        await _service.UpdateAsync(entity.Id, new SpellUpdateDto(null, null, null, null, SpellClass.Bard, null));

        var updated = await _service.GetByIdAsync(entity.Id);
        updated!.Class.Should().Be(SpellClass.Bard);
    }

    [Test]
    public async Task UpdateAsync_WithDescription_ShouldUpdateDescription()
    {
        var dto = CreateSpellCreateDto("Fireball", true, true, false, SpellClass.Wizard, "Fire.");
        await _service.AddAsync(dto);

        var entity = (await _dbContext.Spells.ToListAsync())[0];

        await _service.UpdateAsync(entity.Id, new SpellUpdateDto(null, null, null, null, null, "<p>A brilliant streak flashes.</p>"));

        var updated = await _service.GetByIdAsync(entity.Id);
        updated!.Description.Should().Be("<p>A brilliant streak flashes.</p>");
    }

    [Test]
    public async Task UpdateAsync_NonExistentId_ShouldNotThrow()
    {
        var act = () => _service.UpdateAsync(999, new SpellUpdateDto("Name", true, false, false, SpellClass.Wizard, null));

        await act.Should().NotThrowAsync();

        (await _dbContext.Spells.ToListAsync()).Should().BeEmpty();
    }

    [Test]
    public async Task UpdateAsync_PartialUpdate_ShouldOnlyChangeProvidedFields()
    {
        var dto = CreateSpellCreateDto("Fireball", true, true, false, SpellClass.Wizard, "Fire.");
        await _service.AddAsync(dto);

        var entity = (await _dbContext.Spells.ToListAsync())[0];

        await _service.UpdateAsync(entity.Id, new SpellUpdateDto(null, null, null, true, SpellClass.Sorcerer, null));

        var updated = await _service.GetByIdAsync(entity.Id);
        updated!.Name.Should().Be("Fireball");
        updated.VerbalComponent.Should().BeTrue();
        updated.SomaticComponent.Should().BeTrue();
        updated.MaterialComponent.Should().BeTrue();
        updated.Class.Should().Be(SpellClass.Sorcerer);
        updated.Description.Should().Be("Fire.");
    }

    // DeleteAsync tests

    [Test]
    public async Task DeleteAsync_ExistingId_ShouldRemoveSpell()
    {
        var dto = CreateSpellCreateDto("Fireball", true, true, false, SpellClass.Wizard, "Fire.");
        await _service.AddAsync(dto);

        var entity = (await _dbContext.Spells.ToListAsync())[0];

        await _service.DeleteAsync(entity.Id);

        (await _dbContext.Spells.ToListAsync()).Should().BeEmpty();
    }

    [Test]
    public async Task DeleteAsync_NonExistentId_ShouldNotThrow()
    {
        var act = () => _service.DeleteAsync(999);

        await act.Should().NotThrowAsync();
    }

    // SearchAsync tests

    [Test]
    public async Task SearchAsync_MatchingQuery_ShouldReturnSubset()
    {
        await _service.AddAsync(CreateSpellCreateDto("Fireball", true, true, false, SpellClass.Wizard, "Fire."));
        await _service.AddAsync(CreateSpellCreateDto("Fire Shield", true, true, false, SpellClass.Wizard, "Shield."));
        await _service.AddAsync(CreateSpellCreateDto("Cure Wounds", true, false, false, SpellClass.Cleric, "Heal."));

        var result = await _service.SearchAsync("fire");

        result.Should().HaveCount(2);
        result.Select(e => e.Name).Should().BeEquivalentTo("Fireball", "Fire Shield");
    }

    [Test]
    public async Task SearchAsync_NoMatch_ShouldReturnEmpty()
    {
        await _service.AddAsync(CreateSpellCreateDto("Fireball", true, true, false, SpellClass.Wizard, "Fire."));

        var result = await _service.SearchAsync("healing");

        result.Should().BeEmpty();
    }

    [Test]
    public async Task SearchAsync_EmptyQuery_ShouldReturnAll()
    {
        await _service.AddAsync(CreateSpellCreateDto("Fireball", true, true, false, SpellClass.Wizard, "Fire."));
        await _service.AddAsync(CreateSpellCreateDto("Cure Wounds", true, false, false, SpellClass.Cleric, "Heal."));

        var result = await _service.SearchAsync("");

        result.Should().HaveCount(2);
    }

    [Test]
    public async Task SearchAsync_NullQuery_ShouldReturnAll()
    {
        await _service.AddAsync(CreateSpellCreateDto("Fireball", true, true, false, SpellClass.Wizard, "Fire."));

        var result = await _service.SearchAsync(null!);

        result.Should().HaveCount(1);
    }

    [Test]
    public async Task SearchAsync_CaseInsensitive_ShouldMatch()
    {
        await _service.AddAsync(CreateSpellCreateDto("Fireball", true, true, false, SpellClass.Wizard, "Fire."));

        var result = await _service.SearchAsync("FIREBALL");

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Fireball");
    }

    [Test]
    public async Task SearchAsync_WhitespaceQuery_ShouldReturnAll()
    {
        await _service.AddAsync(CreateSpellCreateDto("Fireball", true, true, false, SpellClass.Wizard, "Fire."));

        var result = await _service.SearchAsync("   ");

        result.Should().HaveCount(1);
    }

    static SpellCreateDto CreateSpellCreateDto(string name, bool verbal, bool somatic, bool material, SpellClass spellClass, string description) =>
        new(name, verbal, somatic, material, spellClass, description);
}
