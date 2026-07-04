using FluentAssertions;
using InitiativeTracker.DataAccess.Dtos;
using InitiativeTracker.DataAccess.Repositories;
using InitiativeTracker.Domain.Enums;
using InitiativeTracker.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace InitiativeTracker.Tests.HandlerTests;

public class MiniatureRepositoryTests
{
    private ILogger<MiniatureRepository> _logger;
    private InitiativeTrackerDbContext _dbContext;
    private MiniatureRepository _repository;

    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<MiniatureRepository>>();
        var options = new DbContextOptionsBuilder<InitiativeTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"MiniatureDb-{Guid.NewGuid()}")
            .Options;
        _dbContext = new InitiativeTrackerDbContext(options);
        _repository = new MiniatureRepository(_logger, _dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Test]
    public async Task AddAsync_ValidDto_ShouldAddMiniature()
    {
        var dto = CreateMiniatureCreateDto("Goblin Commander", CreatureSize.Medium, 123);

        await _repository.AddAsync(dto);

        var result = await _dbContext.Miniatures.ToListAsync();
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Goblin Commander");
        result[0].Size.Should().Be(CreatureSize.Medium);
        result[0].ImageData.Should().NotBeNull();
    }

    [Test]
    public async Task AddAsync_MultipleItems_ShouldPersistAll()
    {
        await _repository.AddAsync(CreateMiniatureCreateDto("Goblin", CreatureSize.Small));
        await _repository.AddAsync(CreateMiniatureCreateDto("Dragon", CreatureSize.Gargantuan));

        var result = await _dbContext.Miniatures.ToListAsync();
        result.Should().HaveCount(2);
    }

    [Test]
    public async Task AddAsync_WithImageData_ShouldPersistImage()
    {
        var imageBytes = new byte[] { 1, 2, 3 };
        var dto = new MiniatureCreateDto("Big Red Dragon", CreatureSize.Gargantuan, imageBytes, [], 12, null, 0, 0, 0, 0);

        await _repository.AddAsync(dto);

        var result = await _dbContext.Miniatures.ToListAsync();
        result.Should().HaveCount(1);
        result[0].ImageData.Should().Equal(imageBytes);
    }

    [Test]
    public async Task GetByIdAsync_ExistingId_ShouldReturnMiniature()
    {
        var dto = CreateMiniatureCreateDto("Goblin", CreatureSize.Small);
        await _repository.AddAsync(dto);

        var result = await _dbContext.Miniatures.ToListAsync();
        var entity = result[0];

        var found = await _repository.GetByIdAsync(entity.Id);

        found.Should().NotBeNull();
        found!.Name.Should().Be("Goblin");
        found.Size.Should().Be(CreatureSize.Small);
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
        var dto = CreateMiniatureCreateDto("Goblin", CreatureSize.Small);
        await _repository.AddAsync(dto);

        var entity = (await _dbContext.Miniatures.ToListAsync())[0];

        await _repository.UpdateAsync(entity.Id, new MiniatureUpdateDto("New Goblin", null, null!, null!, null, null, 0, 0, 0, 0));

        var updated = await _repository.GetByIdAsync(entity.Id);
        updated!.Name.Should().Be("New Goblin");
    }

    [Test]
    public async Task UpdateAsync_NullName_ShouldNotChangeName()
    {
        var dto = CreateMiniatureCreateDto("Goblin", CreatureSize.Small);
        await _repository.AddAsync(dto);

        var entity = (await _dbContext.Miniatures.ToListAsync())[0];

        await _repository.UpdateAsync(entity.Id, new MiniatureUpdateDto(null, null, null!, null!, null, null, 0, 0, 0, 0));

        var updated = await _repository.GetByIdAsync(entity.Id);
        updated!.Name.Should().Be("Goblin");
    }

    [Test]
    public async Task UpdateAsync_WithSize_ShouldUpdateSize()
    {
        var dto = CreateMiniatureCreateDto("Goblin", CreatureSize.Small);
        await _repository.AddAsync(dto);

        var entity = (await _dbContext.Miniatures.ToListAsync())[0];

        await _repository.UpdateAsync(entity.Id, new MiniatureUpdateDto(null, CreatureSize.Medium, null!, null!, null, null, 0, 0, 0, 0));

        var updated = await _repository.GetByIdAsync(entity.Id);
        updated!.Size.Should().Be(CreatureSize.Medium);
    }

    [Test]
    public async Task UpdateAsync_WithPrintedCount_ShouldUpdatePrintedCount()
    {
        var dto = CreateMiniatureCreateDto("Goblin", CreatureSize.Small);
        await _repository.AddAsync(dto);

        var entity = (await _dbContext.Miniatures.ToListAsync())[0];

        await _repository.UpdateAsync(entity.Id, new MiniatureUpdateDto(null, null, null!, null!, 5, null, 0, 0, 0, 0));

        var updated = await _repository.GetByIdAsync(entity.Id);
        updated!.PrintedCount.Should().Be(5);
    }

    [Test]
    public async Task UpdateAsync_WithLink_ShouldUpdateLink()
    {
        var dto = CreateMiniatureCreateDto("Goblin", CreatureSize.Small);
        await _repository.AddAsync(dto);

        var entity = (await _dbContext.Miniatures.ToListAsync())[0];

        await _repository.UpdateAsync(entity.Id, new MiniatureUpdateDto(null, null, null!, null!, null, "link", 0, 0, 0, 0));

        var updated = await _repository.GetByIdAsync(entity.Id);
        updated!.Link.Should().Be("link");
    }

    [Test]
    public async Task UpdateAsync_NonExistentId_ShouldNotThrow()
    {
        var act = () => _repository.UpdateAsync(999, new MiniatureUpdateDto("Name", CreatureSize.Small, null!, null!, null, null, 0, 0, 0, 0));

        await act.Should().NotThrowAsync();

        (await _dbContext.Miniatures.ToListAsync()).Should().BeEmpty();
    }

    [Test]
    public async Task DeleteAsync_ExistingId_ShouldRemoveMiniature()
    {
        var dto = CreateMiniatureCreateDto("Goblin", CreatureSize.Small);
        await _repository.AddAsync(dto);

        var entity = (await _dbContext.Miniatures.ToListAsync())[0];

        await _repository.DeleteAsync(entity.Id);

        (await _dbContext.Miniatures.ToListAsync()).Should().BeEmpty();
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
        await _repository.AddAsync(CreateMiniatureCreateDto("Giant", CreatureSize.Small));
        await _repository.AddAsync(CreateMiniatureCreateDto("Gibbering Mouther", CreatureSize.Medium));
        await _repository.AddAsync(CreateMiniatureCreateDto("Orc", CreatureSize.Large));

        var result = await _repository.SearchAsync("gi");

        result.Should().HaveCount(2);
        result.Select(e => e.Name).Should().BeEquivalentTo("Giant", "Gibbering Mouther");
    }

    [Test]
    public async Task SearchAsync_NoMatch_ShouldReturnEmpty()
    {
        await _repository.AddAsync(CreateMiniatureCreateDto("Goblin", CreatureSize.Small));

        var result = await _repository.SearchAsync("dragon");

        result.Should().BeEmpty();
    }

    [Test]
    public async Task SearchAsync_EmptyQuery_ShouldReturnAll()
    {
        await _repository.AddAsync(CreateMiniatureCreateDto("Goblin", CreatureSize.Small));
        await _repository.AddAsync(CreateMiniatureCreateDto("Orc", CreatureSize.Large));

        var result = await _repository.SearchAsync("");

        result.Should().HaveCount(2);
    }

    [Test]
    public async Task SearchAsync_NullQuery_ShouldReturnAll()
    {
        await _repository.AddAsync(CreateMiniatureCreateDto("Goblin", CreatureSize.Small));

        var result = await _repository.SearchAsync(null!);

        result.Should().HaveCount(1);
    }

    [Test]
    public async Task SearchAsync_CaseInsensitive_ShouldMatch()
    {
        await _repository.AddAsync(CreateMiniatureCreateDto("Goblin", CreatureSize.Small));

        var result = await _repository.SearchAsync("GOBLIN");

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Goblin");
    }

    static MiniatureCreateDto CreateMiniatureCreateDto(
        string name,
        CreatureSize size,
        int printedCount = 0,
        string? link = null) =>
        new(name, size, [], [], printedCount, link, 0, 0, 0, 0);
}
