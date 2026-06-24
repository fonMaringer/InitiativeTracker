using System.Collections.Generic;
using System.Text.RegularExpressions;
using FluentAssertions;
using InitiativeTracker.Application.PrintHtmlGenerators;
using InitiativeTracker.Domain;
using InitiativeTracker.Domain.Enums;

namespace InitiativeTracker.Tests;

public class MiniaturePrintGeneratorTests
{
    private readonly MiniaturePrintGenerator _generator = new();

    const string FakeImageBase64 = "iVBORw0KGgoAAAANSU";

    [Test]
    public void Generate_SingleItem_ShouldReturnValidHtml()
    {
        var items = new[]
        {
            CreateMiniatureDto("Goblin", CreatureSize.Small, 2)
        };

        var html = _generator.Generate(items);

        html.Should().Contain("<!DOCTYPE html>");
        html.Should().Contain("</html>\r\n");
        html.Should().Contain("<head>");
        html.Should().Contain("</head>");
        html.Should().Contain("<body>");
        html.Should().Contain("</body>");
    }

    [Test]
    public void Generate_QuantityOne_ShouldRenderOneCell()
    {
        var items = new[]
        {
            CreateMiniatureDto("Goblin", CreatureSize.Small, 1)
        };

        var html = _generator.Generate(items);

        // Count content cells by looking for slots inside them (padding cells are empty)
        int slotCount = RegexCount(html, "<div class=\"slot");
        int contentCellCount = slotCount / 2;
        contentCellCount.Should().Be(1);
    }

    [Test]
    public void Generate_QuantityTwo_ShouldRenderOneCell()
    {
        var items = new[]
        {
            CreateMiniatureDto("Goblin", CreatureSize.Small, 2)
        };

        var html = _generator.Generate(items);

        int contentCellCount = RegexCount(html, "class=\"cell\" style=");
        contentCellCount.Should().BeGreaterThanOrEqualTo(1);
    }

    [Test]
    public void Generate_QuantityThree_ShouldRenderTwoCells()
    {
        var items = new[]
        {
            CreateMiniatureDto("Goblin", CreatureSize.Small, 3)
        };

        var html = _generator.Generate(items);

        int imgCount = RegexCount(html, "<img");
        imgCount.Should().Be(6);
    }

    [Test]
    public void Generate_TwoImagesPerCell_OneFlipped()
    {
        var items = new[]
        {
            CreateMiniatureDto("Goblin", CreatureSize.Small, 2)
        };

        var html = _generator.Generate(items);

        html.Should().Contain("class=\"slot flipped\"");
        html.Should().Contain("class=\"slot\">");
    }

    [Test]
    public void Generate_ImageBase64Embedded()
    {
        var items = new[]
        {
            new MiniaturePrintDataDto("Goblin", CreatureSize.Small, 2, FakeImageBase64, 0, 0, 0, 0, 0, 0)
        };

        var html = _generator.Generate(items);

        html.Should().Contain($"data:image/png;base64,{FakeImageBase64}");
    }

    [Test]
    public void Generate_TinySize_ShouldUseCorrectDimensions()
    {
        var items = new[]
        {
            CreateMiniatureDto("Insect", CreatureSize.Tiny, 2)
        };

        var html = _generator.Generate(items);

        html.Should().Contain("width:12mm; height:32mm;");
    }

    [Test]
    public void Generate_SmallSize_ShouldUseCorrectDimensions()
    {
        var items = new[]
        {
            CreateMiniatureDto("Goblin", CreatureSize.Small, 2)
        };

        var html = _generator.Generate(items);

        html.Should().Contain("width:25mm; height:64mm;");
    }

    [Test]
    public void Generate_MediumSize_ShouldUseCorrectDimensions()
    {
        var items = new[]
        {
            CreateMiniatureDto("Human", CreatureSize.Medium, 2)
        };

        var html = _generator.Generate(items);

        html.Should().Contain("width:25mm; height:64mm;");
    }

    [Test]
    public void Generate_LargeSize_ShouldUseCorrectDimensions()
    {
        var items = new[]
        {
            CreateMiniatureDto("Ogre", CreatureSize.Large, 2)
        };

        var html = _generator.Generate(items);

        html.Should().Contain("width:50mm; height:128mm;");
    }

    [Test]
    public void Generate_HugeSize_ShouldUseCorrectDimensions()
    {
        var items = new[]
        {
            CreateMiniatureDto("Elemental", CreatureSize.Huge, 2)
        };

        var html = _generator.Generate(items);

        html.Should().Contain("width:75mm; height:192mm;");
    }

    [Test]
    public void Generate_GargantuanSize_ShouldUseCorrectDimensions()
    {
        var items = new[]
        {
            CreateMiniatureDto("Ancient Dragon", CreatureSize.Gargantuan, 2)
        };

        var html = _generator.Generate(items);

        html.Should().Contain("width:100mm; height:256mm;");
    }

    [Test]
    public void Generate_MultipleSizes_ShouldOrderBySize()
    {
        var items = new[]
        {
            CreateMiniatureDto("Dragon", CreatureSize.Gargantuan, 2),
            CreateMiniatureDto("Goblin", CreatureSize.Small, 2),
            CreateMiniatureDto("Insect", CreatureSize.Tiny, 2)
        };

        var html = _generator.Generate(items);

        // Tiny should appear before Small, which should appear before Gargantuan.
        int tinyIndex = html.IndexOf("width:12.5mm; height:32mm;");
        int smallIndex = html.IndexOf("width:25mm; height:64mm;");
        int gargIndex = html.IndexOf("width:100mm; height:256mm;");

        tinyIndex.Should().BeLessThan(smallIndex);
        smallIndex.Should().BeLessThan(gargIndex);
    }

    [Test]
    public void Generate_MultipleSizes_ShouldHaveSeparateSections()
    {
        var items = new[]
        {
            CreateMiniatureDto("Goblin", CreatureSize.Small, 2),
            CreateMiniatureDto("Insect", CreatureSize.Tiny, 2)
        };

        var html = _generator.Generate(items);

        int sectionCount = RegexCount(html, "class=\"sheet\"");
        sectionCount.Should().Be(2);
    }

    [Test]
    public void Generate_SameSize_MultipleItems_ShouldHaveOneSection()
    {
        var items = new[]
        {
            CreateMiniatureDto("Goblin", CreatureSize.Small, 2),
            CreateMiniatureDto("Orc", CreatureSize.Small, 2)
        };

        var html = _generator.Generate(items);

        int sectionCount = RegexCount(html, "class=\"sheet\"");
        sectionCount.Should().Be(1);
    }

    [Test]
    public void Generate_EmptyList_ShouldNotCrash()
    {
        var items = Array.Empty<MiniaturePrintDataDto>();

        var html = _generator.Generate(items);

        html.Should().Contain("<!DOCTYPE html>");
        html.Should().NotContain("class=\"cell\"");
    }

    [Test]
    public void Generate_HighQuantity_ShouldCreateEnoughCells()
    {
        // Quantity 10 → (10+1)/2 = 5 cells → exactly one row of 5, no padding
        var items = new[]
        {
            CreateMiniatureDto("Goblin", CreatureSize.Small, 10)
        };

        var html = _generator.Generate(items);

        int imgCount = RegexCount(html, "<img");
        // 5 cells × 2 images = 10 images
        imgCount.Should().Be(20);
    }

    [Test]
    public void Generate_OddQuantity_High_ShouldRoundUpCells()
    {
        var items = new[]
        {
            CreateMiniatureDto("Goblin", CreatureSize.Small, 9)
        };

        var html = _generator.Generate(items);

        int imgCount = RegexCount(html, "<img");
        imgCount.Should().Be(18);
    }

    [Test]
    public void Generate_PaddingCells_AreEmpty()
    {
        // 1 cell needed, ColumnsOnPage=5 → 4 padding cells
        var items = new[]
        {
            CreateMiniatureDto("Goblin", CreatureSize.Small, 2)
        };

        var html = _generator.Generate(items);

        int totalCells = RegexCount(html, "class=\"cell\"");
        totalCells.Should().BeGreaterThanOrEqualTo(2);
    }

    [Test]
    public void Generate_AllComponentStylesPresent_InHeader()
    {
        var items = new[]
        {
            CreateMiniatureDto("Goblin", CreatureSize.Small, 2)
        };

        var html = _generator.Generate(items);

        html.Should().Contain(".sheet");
        html.Should().Contain(".cell");
        html.Should().Contain(".slot");
        html.Should().Contain(".flipped");
    }

    [Test]
    public void Generate_RotatedSlot_ShouldUseTransform()
    {
        var items = new[]
        {
            CreateMiniatureDto("Goblin", CreatureSize.Small, 2)
        };

        var html = _generator.Generate(items);

        html.Should().Contain("transform: rotateX(180deg)");
    }

    static MiniaturePrintDataDto CreateMiniatureDto(string? name, CreatureSize size, int quantity) =>
        new(name ?? "Unnamed", size, quantity, "iVBORw0KGgoAAAANSU", 0, 0, 0, 0, 0, 0);

    static int RegexCount(string text, string pattern) => Regex.Matches(text, pattern).Count;
}
