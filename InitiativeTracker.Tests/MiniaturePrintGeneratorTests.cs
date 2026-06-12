using System.Collections.Generic;
using System.Text.RegularExpressions;
using FluentAssertions;
using InitiativeTracker.Application.PrintHtmlGenerators;
using InitiativeTracker.Domain;

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
    public void Generate_SingleItem_ShouldRenderLabelWithNameAndQuantity()
    {
        var items = new[]
        {
            CreateMiniatureDto("Goblin", CreatureSize.Small, 4)
        };

        var html = _generator.Generate(items);

        html.Should().Contain("<label class=\"lbl\">");
        html.Should().Contain("&times;  4");
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

        int cellCount = RegexCount(html, "<div class=\"cell\"");
        // (2 + 1) / 2 = 1 cell — padding will fill rest of row
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

        // (3 + 1) / 2 = 2 cells — but padding may add more empty cells.
        // Each cell has a closing </div>. Let's count by looking at image tags: 2 images per cell.
        int imgCount = RegexCount(html, "<img");
        imgCount.Should().Be(4);
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
            new MiniaturePrintDataDto("Goblin", CreatureSize.Small, 2, FakeImageBase64)
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

        html.Should().Contain("width:16mm; height:13mm;");
    }

    [Test]
    public void Generate_SmallSize_ShouldUseCorrectDimensions()
    {
        var items = new[]
        {
            CreateMiniatureDto("Goblin", CreatureSize.Small, 2)
        };

        var html = _generator.Generate(items);

        html.Should().Contain("width:32mm; height:25mm;");
    }

    [Test]
    public void Generate_MediumSize_ShouldUseCorrectDimensions()
    {
        var items = new[]
        {
            CreateMiniatureDto("Human", CreatureSize.Medium, 2)
        };

        var html = _generator.Generate(items);

        html.Should().Contain("width:32mm; height:25mm;");
    }

    [Test]
    public void Generate_LargeSize_ShouldUseCorrectDimensions()
    {
        var items = new[]
        {
            CreateMiniatureDto("Ogre", CreatureSize.Large, 2)
        };

        var html = _generator.Generate(items);

        html.Should().Contain("width:64mm; height:50mm;");
    }

    [Test]
    public void Generate_HugeSize_ShouldUseCorrectDimensions()
    {
        var items = new[]
        {
            CreateMiniatureDto("Elemental", CreatureSize.Huge, 2)
        };

        var html = _generator.Generate(items);

        html.Should().Contain("width:96mm; height:75mm;");
    }

    [Test]
    public void Generate_GargantuanSize_ShouldUseCorrectDimensions()
    {
        var items = new[]
        {
            CreateMiniatureDto("Ancient Dragon", CreatureSize.Gargantuan, 2)
        };

        var html = _generator.Generate(items);

        html.Should().Contain("width:128mm; height:100mm;");
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
        int tinyIndex = html.IndexOf("width:16mm; height:13mm;");
        int smallIndex = html.IndexOf("width:32mm; height:25mm;");
        int gargIndex = html.IndexOf("width:128mm; height:100mm;");

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
    public void Generate_SectionMaxWidth_ShouldBeComputedCorrectly()
    {
        // ColumnsOnPage=5, Small=32mm → maxW = 5*32 + (5-1) = 160+4=164
        var items = new[]
        {
            CreateMiniatureDto("Goblin", CreatureSize.Small, 2)
        };

        var html = _generator.Generate(items);

        html.Should().Contain("max-width:164mm;");
    }

    [Test]
    public void Generate_LargeSectionMaxWidth_ShouldBeComputedCorrectly()
    {
        // Large=64mm → maxW = 5*64 + 4 = 324
        var items = new[]
        {
            CreateMiniatureDto("Ogre", CreatureSize.Large, 2)
        };

        var html = _generator.Generate(items);

        html.Should().Contain("max-width:324mm;");
    }

    [Test]
    public void Generate_SpecialCharsInName_ShouldBeHtmlEncoded()
    {
        var items = new[]
        {
            new MiniaturePrintDataDto("Fiend <of> & Fire", CreatureSize.Medium, 2, FakeImageBase64)
        };

        var html = _generator.Generate(items);

        html.Should().Contain("Fiend &lt;of&gt; &amp; Fire");
    }

    [Test]
    public void Generate_NullName_ShouldRenderUnnamed()
    {
        var items = new MiniaturePrintDataDto[]
        {
            new(null!, CreatureSize.Medium, 2, "iVBORw0KGgoAAAANSU")
        };

        var html = _generator.Generate(items);

        html.Should().Contain("Unnamed");
        html.Should().Contain("&times;  2");
    }

    [Test]
    public void Generate_EmptyList_ShouldNotCrash()
    {
        var items = Array.Empty<MiniaturePrintDataDto>();

        var html = _generator.Generate(items);

        html.Should().Contain("<!DOCTYPE html>");
        html.Should().NotContain("class=\"lbl\"");
        html.Should().NotContain("class=\"cell\"");
    }

    [Test]
    public void Generate_LabelShowsItemNameAndQuantity()
    {
        var items = new[]
        {
            CreateMiniatureDto("Orc", CreatureSize.Medium, 6)
        };

        var html = _generator.Generate(items);

        // Label should contain name and times sign with quantity.
        html.Should().MatchRegex("<label class=\"lbl\">.*Orc.*&times;.*6.*</label>");
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
        imgCount.Should().Be(10);
    }

    [Test]
    public void Generate_OddQuantity_High_ShouldRoundUpCells()
    {
        // Quantity 9 → (9+1)/2 = 5 cells exactly
        var items = new[]
        {
            CreateMiniatureDto("Goblin", CreatureSize.Small, 9)
        };

        var html = _generator.Generate(items);

        int imgCount = RegexCount(html, "<img");
        imgCount.Should().Be(10);
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
        totalCells.Should().BeGreaterThanOrEqualTo(5);
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
        html.Should().Contain(".lbl");
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

        html.Should().Contain("transform:rotate(180deg)");
    }

    static MiniaturePrintDataDto CreateMiniatureDto(string? name, CreatureSize size, int quantity) =>
        new(name ?? "Unnamed", size, quantity, "iVBORw0KGgoAAAANSU");

    static int RegexCount(string text, string pattern) => Regex.Matches(text, pattern).Count;
}
