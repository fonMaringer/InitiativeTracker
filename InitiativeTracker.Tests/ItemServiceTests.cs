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

public class ItemServiceTests
{
    private ILogger<ItemService> _logger;
    private InitiativeTrackerDbContext _dbContext;
    private ItemService _service;

    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<ItemService>>();
        var options = new DbContextOptionsBuilder<InitiativeTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"ItemDb-{Guid.NewGuid()}")
            .Options;
        _dbContext = new InitiativeTrackerDbContext(options);
        _service = new ItemService(_logger, _dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Test]
    public async Task AddAsync_ValidDto_ShouldAddItem()
    {
        var dto = CreateItemCreateDto("Dagger", ItemRarity.Common, false, "A small bladed weapon.");

        await _service.AddAsync(dto);

        var result = await _dbContext.Items.ToListAsync();
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Dagger");
        result[0].Rarity.Should().Be(ItemRarity.Common);
        result[0].RequiresAttunement.Should().BeFalse();
        result[0].Description.Should().Be("A small bladed weapon.");
    }

    [Test]
    public async Task AddAsync_WithAttunement_ShouldPersistFlag()
    {
        var dto = CreateItemCreateDto("Cloak of Protection", ItemRarity.Rare, true, "+1 AC and saving throws.");

        await _service.AddAsync(dto);

        var result = await _dbContext.Items.ToListAsync();
        result.Should().HaveCount(1);
        result[0].RequiresAttunement.Should().BeTrue();
    }

    [Test]
    public async Task AddAsync_MultipleItems_ShouldPersistAll()
    {
        await _service.AddAsync(CreateItemCreateDto("Dagger", ItemRarity.Common, false, "A dagger."));
        await _service.AddAsync(CreateItemCreateDto("Plate Armor", ItemRarity.Uncommon, false, "Heavy armor."));

        var result = await _dbContext.Items.ToListAsync();
        result.Should().HaveCount(2);
    }

    [Test]
    public async Task AddAsync_LegendaryRarity_ShouldPersist()
    {
        var dto = CreateItemCreateDto("Vorpal Sword", ItemRarity.Legendary, true, "Decapitates on a natural 20.");

        await _service.AddAsync(dto);

        var result = await _dbContext.Items.ToListAsync();
        result.Should().HaveCount(1);
        result[0].Rarity.Should().Be(ItemRarity.Legendary);
    }

    [Test]
    public async Task GetByIdAsync_ExistingId_ShouldReturnItem()
    {
        var dto = CreateItemCreateDto("Dagger", ItemRarity.Common, false, "A dagger.");
        await _service.AddAsync(dto);

        var entity = (await _dbContext.Items.ToListAsync())[0];
        var found = await _service.GetByIdAsync(entity.Id);

        found.Should().NotBeNull();
        found!.Name.Should().Be("Dagger");
        found.Rarity.Should().Be(ItemRarity.Common);
    }

    [Test]
    public async Task GetByIdAsync_NonExistentId_ShouldReturnNull()
    {
        var found = await _service.GetByIdAsync(999);

        found.Should().BeNull();
    }

    [Test]
    public async Task UpdateAsync_ExistingName_ShouldUpdateName()
    {
        var dto = CreateItemCreateDto("Dagger", ItemRarity.Common, false, "A dagger.");
        await _service.AddAsync(dto);

        var entity = (await _dbContext.Items.ToListAsync())[0];

        await _service.UpdateAsync(entity.Id, new ItemUpdateDto("Main-Gauche", null, null, null, null));

        var updated = await _service.GetByIdAsync(entity.Id);
        updated!.Name.Should().Be("Main-Gauche");
    }

    [Test]
    public async Task UpdateAsync_NullName_ShouldNotChangeName()
    {
        var dto = CreateItemCreateDto("Dagger", ItemRarity.Common, false, "A dagger.");
        await _service.AddAsync(dto);

        var entity = (await _dbContext.Items.ToListAsync())[0];

        await _service.UpdateAsync(entity.Id, new ItemUpdateDto(null, null, null, null, null));

        var updated = await _service.GetByIdAsync(entity.Id);
        updated!.Name.Should().Be("Dagger");
    }

    [Test]
    public async Task UpdateAsync_WithRarity_ShouldUpdateRarity()
    {
        var dto = CreateItemCreateDto("Dagger", ItemRarity.Common, false, "A dagger.");
        await _service.AddAsync(dto);

        var entity = (await _dbContext.Items.ToListAsync())[0];

        await _service.UpdateAsync(entity.Id, new ItemUpdateDto(null, ItemRarity.Uncommon, null, null, null));

        var updated = await _service.GetByIdAsync(entity.Id);
        updated!.Rarity.Should().Be(ItemRarity.Uncommon);
    }

    [Test]
    public async Task UpdateAsync_WithAttunement_ShouldUpdateAttunement()
    {
        var dto = CreateItemCreateDto("Dagger", ItemRarity.Common, false, "A dagger.");
        await _service.AddAsync(dto);

        var entity = (await _dbContext.Items.ToListAsync())[0];

        await _service.UpdateAsync(entity.Id, new ItemUpdateDto(null, null, true, null, null));

        var updated = await _service.GetByIdAsync(entity.Id);
        updated!.RequiresAttunement.Should().BeTrue();
    }

    [Test]
    public async Task UpdateAsync_WithDescription_ShouldUpdateDescription()
    {
        var dto = CreateItemCreateDto("Dagger", ItemRarity.Common, false, "A dagger.");
        await _service.AddAsync(dto);

        var entity = (await _dbContext.Items.ToListAsync())[0];

        await _service.UpdateAsync(entity.Id, new ItemUpdateDto(null, null, null, "<p>Magical +1 dagger.</p>", null));

        var updated = await _service.GetByIdAsync(entity.Id);
        updated!.Description.Should().Be("<p>Magical +1 dagger.</p>");
    }

    [Test]
    public async Task UpdateAsync_WithPrintedCount_ShouldUpdateDescription()
    {
        var dto = CreateItemCreateDto("Dagger", ItemRarity.Common, false, "A dagger.");
        await _service.AddAsync(dto);

        var entity = (await _dbContext.Items.ToListAsync())[0];

        await _service.UpdateAsync(entity.Id, new ItemUpdateDto(null, null, null,  null, 100));

        var updated = await _service.GetByIdAsync(entity.Id);
        updated!.PrintedCount.Should().Be(100);
    }

    [Test]
    public async Task UpdateAsync_NonExistentId_ShouldNotThrow()
    {
        var act = () => _service.UpdateAsync(999, new ItemUpdateDto("Name", ItemRarity.Common, false, null, null));

        await act.Should().NotThrowAsync();

        (await _dbContext.Items.ToListAsync()).Should().BeEmpty();
    }

    [Test]
    public async Task UpdateAsync_PartialUpdate_ShouldOnlyChangeProvidedFields()
    {
        var dto = CreateItemCreateDto("Dagger", ItemRarity.Common, false, "A dagger.");
        await _service.AddAsync(dto);

        var entity = (await _dbContext.Items.ToListAsync())[0];

        await _service.UpdateAsync(entity.Id, new ItemUpdateDto(null, ItemRarity.Rare, null, null, null));

        var updated = await _service.GetByIdAsync(entity.Id);
        updated!.Name.Should().Be("Dagger");
        updated.Rarity.Should().Be(ItemRarity.Rare);
        updated.RequiresAttunement.Should().BeFalse();
        updated.Description.Should().Be("A dagger.");
    }

    [Test]
    public async Task DeleteAsync_ExistingId_ShouldRemoveItem()
    {
        var dto = CreateItemCreateDto("Dagger", ItemRarity.Common, false, "A dagger.");
        await _service.AddAsync(dto);

        var entity = (await _dbContext.Items.ToListAsync())[0];

        await _service.DeleteAsync(entity.Id);

        (await _dbContext.Items.ToListAsync()).Should().BeEmpty();
    }

    [Test]
    public async Task DeleteAsync_NonExistentId_ShouldNotThrow()
    {
        var act = () => _service.DeleteAsync(999);

        await act.Should().NotThrowAsync();
    }

    [Test]
    public async Task SearchAsync_MatchingQuery_ShouldReturnSubset()
    {
        await _service.AddAsync(CreateItemCreateDto("Longsword", ItemRarity.Common, false, "A sword."));
        await _service.AddAsync(CreateItemCreateDto("Shortsword", ItemRarity.Common, false, "A short sword."));
        await _service.AddAsync(CreateItemCreateDto("Plate Armor", ItemRarity.Uncommon, false, "Heavy armor."));

        var result = await _service.SearchAsync("sword");

        result.Should().HaveCount(2);
        result.Select(e => e.Name).Should().BeEquivalentTo("Longsword", "Shortsword");
    }

    [Test]
    public async Task SearchAsync_NoMatch_ShouldReturnEmpty()
    {
        await _service.AddAsync(CreateItemCreateDto("Dagger", ItemRarity.Common, false, "A dagger."));

        var result = await _service.SearchAsync("staff");

        result.Should().BeEmpty();
    }

    [Test]
    public async Task SearchAsync_EmptyQuery_ShouldReturnAll()
    {
        await _service.AddAsync(CreateItemCreateDto("Dagger", ItemRarity.Common, false, "A dagger."));
        await _service.AddAsync(CreateItemCreateDto("Plate Armor", ItemRarity.Uncommon, false, "Heavy armor."));

        var result = await _service.SearchAsync("");

        result.Should().HaveCount(2);
    }

    [Test]
    public async Task SearchAsync_NullQuery_ShouldReturnAll()
    {
        await _service.AddAsync(CreateItemCreateDto("Dagger", ItemRarity.Common, false, "A dagger."));

        var result = await _service.SearchAsync(null!);

        result.Should().HaveCount(1);
    }

    [Test]
    public async Task SearchAsync_CaseInsensitive_ShouldMatch()
    {
        await _service.AddAsync(CreateItemCreateDto("Dagger", ItemRarity.Common, false, "A dagger."));

        var result = await _service.SearchAsync("DAGGER");

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Dagger");
    }

    [Test]
    public async Task AddAsync_UndefinedRarity_ShouldPersist()
    {
        var dto = CreateItemCreateDto("Mystery Item", ItemRarity.Undefined, false, "Unknown description.");

        await _service.AddAsync(dto);

        var result = await _dbContext.Items.ToListAsync();
        result.Should().HaveCount(1);
        result[0].Rarity.Should().Be(ItemRarity.Undefined);
    }

    [Test]
    public async Task AddAsync_HtmlDescription_ShouldPersist()
    {
        var htmlDesc = "<h4>Properties</h4><ul><li>+1 AC</li><li>Resistance to fire</li></ul>";
        var dto = CreateItemCreateDto("Shield", ItemRarity.Uncommon, true, htmlDesc);

        await _service.AddAsync(dto);

        var result = await _dbContext.Items.ToListAsync();
        result.Should().HaveCount(1);
        result[0].Description.Should().Be(htmlDesc);
    }

    static ItemCreateDto CreateItemCreateDto(string name, ItemRarity rarity, bool requiresAttunement, string description, int printedCount = 0) =>
        new(name, rarity, requiresAttunement, description, printedCount);
}
