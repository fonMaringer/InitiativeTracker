using FluentAssertions;
using InitiativeTracker.Application;
using InitiativeTracker.Application.Dtos;
using InitiativeTracker.Domain;
using InitiativeTracker.Domain.Entities;
using InitiativeTracker.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace InitiativeTracker.Tests;

public class MiniatureServiceTests
{
    private ILogger<MiniatureService> _logger;
    private InitiativeTrackerDbContext _dbContext;
    private MiniatureService _service;

    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<MiniatureService>>();
        var options = new DbContextOptionsBuilder<InitiativeTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: $"MiniatureDb-{Guid.NewGuid()}")
            .Options;
        _dbContext = new InitiativeTrackerDbContext(options);
        _service = new MiniatureService(_logger, _dbContext);
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
        var dto = CreateMiniatureCreateDto("Goblin Commander", CreatureSize.Medium, new CropRegion(10, 20, 50, 60));

        await _service.AddAsync(dto);

        var result = await _dbContext.Miniatures.ToListAsync();
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Goblin Commander");
        result[0].Size.Should().Be(CreatureSize.Medium);
        result[0].ImageData.Should().NotBeNull();
    }

    [Test]
    public async Task AddAsync_WithNullCropRegion_ShouldUseDefaults()
    {
        var dto = CreateMiniatureCreateDto("Orc", CreatureSize.Large, null);

        await _service.AddAsync(dto);

        var result = await _dbContext.Miniatures.ToListAsync();
        result[0].CroppedRegionX.Should().Be(0);
        result[0].CroppedRegionY.Should().Be(0);
        result[0].CroppedRegionWidth.Should().Be(100);
        result[0].CroppedRegionHeight.Should().Be(100);
    }

    [Test]
    public async Task AddAsync_MultipleItems_ShouldPersistAll()
    {
        await _service.AddAsync(CreateMiniatureCreateDto("Goblin", CreatureSize.Small, null));
        await _service.AddAsync(CreateMiniatureCreateDto("Dragon", CreatureSize.Gargantuan, null));

        var result = await _dbContext.Miniatures.ToListAsync();
        result.Should().HaveCount(2);
    }

    [Test]
    public async Task AddAsync_WithImageData_ShouldPersistImage()
    {
        var imageBytes = new byte[] { 1, 2, 3 };
        var dto = new MiniatureCreateDto("Big Red Dragon", CreatureSize.Gargantuan, imageBytes, null);

        await _service.AddAsync(dto);

        var result = await _dbContext.Miniatures.ToListAsync();
        result.Should().HaveCount(1);
        result[0].ImageData.Should().Equal(imageBytes);
    }

    [Test]
    public async Task GetByIdAsync_ExistingId_ShouldReturnMiniature()
    {
        var dto = CreateMiniatureCreateDto("Goblin", CreatureSize.Small, null);
        await _service.AddAsync(dto);

        var result = await _dbContext.Miniatures.ToListAsync();
        var entity = result[0];

        var found = await _service.GetByIdAsync(entity.Id);

        found.Should().NotBeNull();
        found!.Name.Should().Be("Goblin");
        found.Size.Should().Be(CreatureSize.Small);
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
        var dto = CreateMiniatureCreateDto("Goblin", CreatureSize.Small, null);
        await _service.AddAsync(dto);

        var entity = (await _dbContext.Miniatures.ToListAsync())[0];

        await _service.UpdateAsync(entity.Id, new MiniatureUpdateDto("New Goblin", null, null));

        var updated = await _service.GetByIdAsync(entity.Id);
        updated!.Name.Should().Be("New Goblin");
    }

    [Test]
    public async Task UpdateAsync_NullName_ShouldNotChangeName()
    {
        var dto = CreateMiniatureCreateDto("Goblin", CreatureSize.Small, null);
        await _service.AddAsync(dto);

        var entity = (await _dbContext.Miniatures.ToListAsync())[0];

        await _service.UpdateAsync(entity.Id, new MiniatureUpdateDto(null, null, null));

        var updated = await _service.GetByIdAsync(entity.Id);
        updated!.Name.Should().Be("Goblin");
    }

    [Test]
    public async Task UpdateAsync_WithSize_ShouldUpdateSize()
    {
        var dto = CreateMiniatureCreateDto("Goblin", CreatureSize.Small, null);
        await _service.AddAsync(dto);

        var entity = (await _dbContext.Miniatures.ToListAsync())[0];

        await _service.UpdateAsync(entity.Id, new MiniatureUpdateDto(null, CreatureSize.Medium, null));

        var updated = await _service.GetByIdAsync(entity.Id);
        updated!.Size.Should().Be(CreatureSize.Medium);
    }

    [Test]
    public async Task UpdateAsync_WithCropRegion_ShouldUpdateCropRegion()
    {
        var dto = CreateMiniatureCreateDto("Goblin", CreatureSize.Small, null);
        await _service.AddAsync(dto);

        var entity = (await _dbContext.Miniatures.ToListAsync())[0];

        var cropRegion = new CropRegion(5, 10, 80, 90);
        await _service.UpdateAsync(entity.Id, new MiniatureUpdateDto(null, null, cropRegion));

        var updated = await _service.GetByIdAsync(entity.Id);
        updated!.CroppedRegionX.Should().Be(5);
        updated.CroppedRegionY.Should().Be(10);
        updated.CroppedRegionWidth.Should().Be(80);
        updated.CroppedRegionHeight.Should().Be(90);
    }

    [Test]
    public async Task UpdateAsync_NonExistentId_ShouldNotThrow()
    {
        var act = () => _service.UpdateAsync(999, new MiniatureUpdateDto("Name", CreatureSize.Small, null));

        await act.Should().NotThrowAsync();

        (await _dbContext.Miniatures.ToListAsync()).Should().BeEmpty();
    }

    [Test]
    public async Task DeleteAsync_ExistingId_ShouldRemoveMiniature()
    {
        var dto = CreateMiniatureCreateDto("Goblin", CreatureSize.Small, null);
        await _service.AddAsync(dto);

        var entity = (await _dbContext.Miniatures.ToListAsync())[0];

        await _service.DeleteAsync(entity.Id);

        (await _dbContext.Miniatures.ToListAsync()).Should().BeEmpty();
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
        await _service.AddAsync(CreateMiniatureCreateDto("Giant", CreatureSize.Small, null));
        await _service.AddAsync(CreateMiniatureCreateDto("Gibbering Mouther", CreatureSize.Medium, null));
        await _service.AddAsync(CreateMiniatureCreateDto("Orc", CreatureSize.Large, null));

        var result = await _service.SearchAsync("gi");

        result.Should().HaveCount(2);
        result.Select(e => e.Name).Should().BeEquivalentTo("Giant", "Gibbering Mouther");
    }

    [Test]
    public async Task SearchAsync_NoMatch_ShouldReturnEmpty()
    {
        await _service.AddAsync(CreateMiniatureCreateDto("Goblin", CreatureSize.Small, null));

        var result = await _service.SearchAsync("dragon");

        result.Should().BeEmpty();
    }

    [Test]
    public async Task SearchAsync_EmptyQuery_ShouldReturnAll()
    {
        await _service.AddAsync(CreateMiniatureCreateDto("Goblin", CreatureSize.Small, null));
        await _service.AddAsync(CreateMiniatureCreateDto("Orc", CreatureSize.Large, null));

        var result = await _service.SearchAsync("");

        result.Should().HaveCount(2);
    }

    [Test]
    public async Task SearchAsync_NullQuery_ShouldReturnAll()
    {
        await _service.AddAsync(CreateMiniatureCreateDto("Goblin", CreatureSize.Small, null));

        var result = await _service.SearchAsync(null!);

        result.Should().HaveCount(1);
    }

    [Test]
    public async Task SearchAsync_CaseInsensitive_ShouldMatch()
    {
        await _service.AddAsync(CreateMiniatureCreateDto("Goblin", CreatureSize.Small, null));

        var result = await _service.SearchAsync("GOBLIN");

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Goblin");
    }

    [Test]
    public async Task GetImageAsync_NonExistentId_ShouldReturnEmptyArray()
    {
        var result = await _service.GetImageAsync(999);

        result.Should().BeEmpty();
    }

    static MiniatureCreateDto CreateMiniatureCreateDto(string name, CreatureSize size, CropRegion? cropRegion) =>
        new(name, size, Array.Empty<byte>(), cropRegion);
}
