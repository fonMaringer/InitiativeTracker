using FluentAssertions;
using InitiativeTracker.DataAccess.Dtos;
using InitiativeTracker.DataAccess.Repositories;
using InitiativeTracker.Domain.Enums;
using InitiativeTracker.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace InitiativeTracker.Tests.HandlerTests;

public class MagicMagicItemRepositoryTests
{
    private ILogger<MagicMagicItemRepository> _logger;
    private InitiativeTrackerDbContext _dbContext;
    private MagicMagicItemRepository _repository;

    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<MagicMagicItemRepository>>();
        var options = new DbContextOptionsBuilder<InitiativeTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"ItemDb-{Guid.NewGuid()}")
            .Options;
        _dbContext = new InitiativeTrackerDbContext(options);
        _repository = new MagicMagicItemRepository(_logger, _dbContext);
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
        var dto = CreateItemCreateDto("Dagger", null, ItemRarity.Common, false, "A small bladed weapon.");

        await _repository.AddAsync(dto);

        var result = await _dbContext.MagicItems.ToListAsync();
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Dagger");
        result[0].Rarity.Should().Be(ItemRarity.Common);
        result[0].RequiresAttunement.Should().BeFalse();
        result[0].Description.Should().Be("A small bladed weapon.");
    }

    [Test]
    public async Task AddAsync_WithAttunement_ShouldPersistFlag()
    {
        var dto = CreateItemCreateDto("Cloak of Protection", null, ItemRarity.Rare, true, "+1 AC and saving throws.");

        await _repository.AddAsync(dto);

        var result = await _dbContext.MagicItems.ToListAsync();
        result.Should().HaveCount(1);
        result[0].RequiresAttunement.Should().BeTrue();
    }

    [Test]
    public async Task AddAsync_MultipleItems_ShouldPersistAll()
    {
        await _repository.AddAsync(CreateItemCreateDto("Dagger", null, ItemRarity.Common, false, "A dagger."));
        await _repository.AddAsync(CreateItemCreateDto("Plate Armor", null, ItemRarity.Uncommon, false, "Heavy armor."));

        var result = await _dbContext.MagicItems.ToListAsync();
        result.Should().HaveCount(2);
    }

    [Test]
    public async Task AddAsync_LegendaryRarity_ShouldPersist()
    {
        var dto = CreateItemCreateDto("Vorpal Sword", null, ItemRarity.Legendary, true, "Decapitates on a natural 20.");

        await _repository.AddAsync(dto);

        var result = await _dbContext.MagicItems.ToListAsync();
        result.Should().HaveCount(1);
        result[0].Rarity.Should().Be(ItemRarity.Legendary);
    }

    [Test]
    public async Task GetByIdAsync_ExistingId_ShouldReturnItem()
    {
        var dto = CreateItemCreateDto("Dagger", null, ItemRarity.Common, false, "A dagger.");
        await _repository.AddAsync(dto);

        var entity = (await _dbContext.MagicItems.ToListAsync())[0];
        var found = await _repository.GetByIdAsync(entity.Id);

        found.Should().NotBeNull();
        found!.Name.Should().Be("Dagger");
        found.Rarity.Should().Be(ItemRarity.Common);
    }

    [Test]
    public async Task GetByIdAsync_NonExistentId_ShouldReturnNull()
    {
        var found = await _repository.GetByIdAsync(999);

        found.Should().BeNull();
    }

    [Test]
    public async Task UpdateAsync_ExistingName_ShouldUpdateName()
    {
        var dto = CreateItemCreateDto("Dagger", null, ItemRarity.Common, false, "A dagger.");
        await _repository.AddAsync(dto);

        var entity = (await _dbContext.MagicItems.ToListAsync())[0];

        await _repository.UpdateAsync(entity.Id, new MagicItemUpdateDto("Main-Gauche", null, null, null, null, null, null));

        var updated = await _repository.GetByIdAsync(entity.Id);
        updated!.Name.Should().Be("Main-Gauche");
    }

    [Test]
    public async Task UpdateAsync_NullName_ShouldNotChangeName()
    {
        var dto = CreateItemCreateDto("Dagger", null, ItemRarity.Common, false, "A dagger.");
        await _repository.AddAsync(dto);

        var entity = (await _dbContext.MagicItems.ToListAsync())[0];

        await _repository.UpdateAsync(entity.Id, new MagicItemUpdateDto(null, null, null, null, null, null, null));

        var updated = await _repository.GetByIdAsync(entity.Id);
        updated!.Name.Should().Be("Dagger");
    }

    [Test]
    public async Task UpdateAsync_WithRarity_ShouldUpdateRarity()
    {
        var dto = CreateItemCreateDto("Dagger", null, ItemRarity.Common, false, "A dagger.");
        await _repository.AddAsync(dto);

        var entity = (await _dbContext.MagicItems.ToListAsync())[0];

        await _repository.UpdateAsync(entity.Id, new MagicItemUpdateDto(null, null, ItemRarity.Uncommon, null, null, null, null));

        var updated = await _repository.GetByIdAsync(entity.Id);
        updated!.Rarity.Should().Be(ItemRarity.Uncommon);
    }

    [Test]
    public async Task UpdateAsync_WithAttunement_ShouldUpdateAttunement()
    {
        var dto = CreateItemCreateDto("Dagger", null, ItemRarity.Common, false, "A dagger.");
        await _repository.AddAsync(dto);

        var entity = (await _dbContext.MagicItems.ToListAsync())[0];

        await _repository.UpdateAsync(entity.Id, new MagicItemUpdateDto(null, null, null, true, null, null, null));

        var updated = await _repository.GetByIdAsync(entity.Id);
        updated!.RequiresAttunement.Should().BeTrue();
    }

    [Test]
    public async Task UpdateAsync_WithType_ShouldUpdateDescription()
    {
        var dto = CreateItemCreateDto("Dagger", "Some", ItemRarity.Common, false, "A dagger.");
        await _repository.AddAsync(dto);

        var entity = (await _dbContext.MagicItems.ToListAsync())[0];

        await _repository.UpdateAsync(entity.Id, new MagicItemUpdateDto(null, "Another", null, null, null, null, null));

        var updated = await _repository.GetByIdAsync(entity.Id);
        updated!.Type.Should().Be("Another");
    }

    [Test]
    public async Task UpdateAsync_WithDescription_ShouldUpdateDescription()
    {
        var dto = CreateItemCreateDto("Dagger", null, ItemRarity.Common, false, "A dagger.");
        await _repository.AddAsync(dto);

        var entity = (await _dbContext.MagicItems.ToListAsync())[0];

        await _repository.UpdateAsync(entity.Id, new MagicItemUpdateDto(null, null, null, null, "<p>Magical +1 dagger.</p>", null, null));

        var updated = await _repository.GetByIdAsync(entity.Id);
        updated!.Description.Should().Be("<p>Magical +1 dagger.</p>");
    }

    [Test]
    public async Task UpdateAsync_WithPrintedCount_ShouldUpdatePrintedCount()
    {
        var dto = CreateItemCreateDto("Dagger", null, ItemRarity.Common, false, "A dagger.");
        await _repository.AddAsync(dto);

        var entity = (await _dbContext.MagicItems.ToListAsync())[0];

        await _repository.UpdateAsync(entity.Id, new MagicItemUpdateDto(null, null, null, null,  null, 100, null));

        var updated = await _repository.GetByIdAsync(entity.Id);
        updated!.PrintedCount.Should().Be(100);
    }

    [Test]
    public async Task UpdateAsync_WithLink_ShouldUpdateLink()
    {
        var dto = CreateItemCreateDto("Dagger", null, ItemRarity.Common, false, "A dagger.");
        await _repository.AddAsync(dto);

        var entity = (await _dbContext.MagicItems.ToListAsync())[0];

        await _repository.UpdateAsync(entity.Id, new MagicItemUpdateDto(null, null, null, null,  null, null, "link"));

        var updated = await _repository.GetByIdAsync(entity.Id);
        updated!.Link.Should().Be("link");
    }

    [Test]
    public async Task UpdateAsync_NonExistentId_ShouldNotThrow()
    {
        var act = () => _repository.UpdateAsync(999, new MagicItemUpdateDto("Name", null, ItemRarity.Common, false, null, null, null));

        await act.Should().NotThrowAsync();

        (await _dbContext.MagicItems.ToListAsync()).Should().BeEmpty();
    }

    [Test]
    public async Task UpdateAsync_PartialUpdate_ShouldOnlyChangeProvidedFields()
    {
        var dto = CreateItemCreateDto("Dagger", null, ItemRarity.Common, false, "A dagger.");
        await _repository.AddAsync(dto);

        var entity = (await _dbContext.MagicItems.ToListAsync())[0];

        await _repository.UpdateAsync(entity.Id, new MagicItemUpdateDto(null, null, ItemRarity.Rare, null, null, null, null));

        var updated = await _repository.GetByIdAsync(entity.Id);
        updated!.Name.Should().Be("Dagger");
        updated.Rarity.Should().Be(ItemRarity.Rare);
        updated.RequiresAttunement.Should().BeFalse();
        updated.Description.Should().Be("A dagger.");
    }

    [Test]
    public async Task DeleteAsync_ExistingId_ShouldRemoveItem()
    {
        var dto = CreateItemCreateDto("Dagger", null, ItemRarity.Common, false, "A dagger.");
        await _repository.AddAsync(dto);

        var entity = (await _dbContext.MagicItems.ToListAsync())[0];

        await _repository.DeleteAsync(entity.Id);

        (await _dbContext.MagicItems.ToListAsync()).Should().BeEmpty();
    }

    [Test]
    public async Task DeleteAsync_NonExistentId_ShouldNotThrow()
    {
        var act = () => _repository.DeleteAsync(999);

        await act.Should().NotThrowAsync();
    }

    [Test]
    public async Task SearchAsync_MatchingQuery_ShouldReturnSubset()
    {
        await _repository.AddAsync(CreateItemCreateDto("Longsword", null, ItemRarity.Common, false, "A sword."));
        await _repository.AddAsync(CreateItemCreateDto("Shortsword", null, ItemRarity.Common, false, "A short sword."));
        await _repository.AddAsync(CreateItemCreateDto("Plate Armor", null, ItemRarity.Uncommon, false, "Heavy armor."));

        var result = await _repository.SearchAsync("sword");

        result.Should().HaveCount(2);
        result.Select(e => e.Name).Should().BeEquivalentTo("Longsword", "Shortsword");
    }

    [Test]
    public async Task SearchAsync_NoMatch_ShouldReturnEmpty()
    {
        await _repository.AddAsync(CreateItemCreateDto("Dagger", null, ItemRarity.Common, false, "A dagger."));

        var result = await _repository.SearchAsync("staff");

        result.Should().BeEmpty();
    }

    [Test]
    public async Task SearchAsync_EmptyQuery_ShouldReturnAll()
    {
        await _repository.AddAsync(CreateItemCreateDto("Dagger", null, ItemRarity.Common, false, "A dagger."));
        await _repository.AddAsync(CreateItemCreateDto("Plate Armor", null, ItemRarity.Uncommon, false, "Heavy armor."));

        var result = await _repository.SearchAsync("");

        result.Should().HaveCount(2);
    }

    [Test]
    public async Task SearchAsync_NullQuery_ShouldReturnAll()
    {
        await _repository.AddAsync(CreateItemCreateDto("Dagger", null, ItemRarity.Common, false, "A dagger."));

        var result = await _repository.SearchAsync(null!);

        result.Should().HaveCount(1);
    }

    [Test]
    public async Task SearchAsync_CaseInsensitive_ShouldMatch()
    {
        await _repository.AddAsync(CreateItemCreateDto("Dagger", null, ItemRarity.Common, false, "A dagger."));

        var result = await _repository.SearchAsync("DAGGER");

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Dagger");
    }

    [Test]
    public async Task AddAsync_UndefinedRarity_ShouldPersist()
    {
        var dto = CreateItemCreateDto("Mystery Item", null, ItemRarity.Unknown, false, "Unknown description.");

        await _repository.AddAsync(dto);

        var result = await _dbContext.MagicItems.ToListAsync();
        result.Should().HaveCount(1);
        result[0].Rarity.Should().Be(ItemRarity.Unknown);
    }

    [Test]
    public async Task AddAsync_HtmlDescription_ShouldPersist()
    {
        var htmlDesc = "<h4>Properties</h4><ul><li>+1 AC</li><li>Resistance to fire</li></ul>";
        var dto = CreateItemCreateDto("Shield", null, ItemRarity.Uncommon, true, htmlDesc);

        await _repository.AddAsync(dto);

        var result = await _dbContext.MagicItems.ToListAsync();
        result.Should().HaveCount(1);
        result[0].Description.Should().Be(htmlDesc);
    }

    static MagicItemCreateDto CreateItemCreateDto(
        string name,
        string? type,
        ItemRarity rarity,
        bool requiresAttunement,
        string description,
        int printedCount = 0,
        string? link = null,
        Source source = Source.Manual) =>
        new(name, type, rarity, requiresAttunement, description, printedCount, link, source);
}
