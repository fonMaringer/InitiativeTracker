using System.Collections.Generic;
using System.Text.RegularExpressions;
using FluentAssertions;
using InitiativeTracker.Application.PrintHtmlGenerators;
using InitiativeTracker.Domain;
using InitiativeTracker.Domain.Enums;

namespace InitiativeTracker.Tests;

public class ItemPrintGeneratorTests
{
    private readonly ItemPrintGenerator _generator = new();

    [Test]
    public void Generate_SingleItem_ShouldReturnHtmlWithCard()
    {
        var items = new[]
        {
            CreateItemDto("Dagger", ItemRarity.Common, false, "A small bladed weapon.")
        };

        var html = _generator.Generate(items);

        html.Should().Contain("<!DOCTYPE html>");
        html.Should().Contain("\"poker-card\"");
        html.Should().Contain("Dagger");
        html.Should().Contain("Common");
    }

    [Test]
    public void Generate_WithAttunement_ShouldRenderBadge()
    {
        var items = new[]
        {
            CreateItemDto("Cloak of Protection", ItemRarity.Rare, true, "+1 AC and saving throws.")
        };

        var html = _generator.Generate(items);

        html.Should().Contain("\"attunement-badge\">ATT");
    }

    [Test]
    public void Generate_WithoutAttunement_ShouldNotRenderBadge()
    {
        var items = new[]
        {
            CreateItemDto("Dagger", ItemRarity.Common, false, "A small bladed weapon.")
        };

        var html = _generator.Generate(items);

        html.Should().NotContain("\"attunement-badge\"");
    }

    [Test]
    public void Generate_MultipleItems_ShouldRenderAllCards()
    {
        var items = new[]
        {
            CreateItemDto("Dagger", ItemRarity.Common, false, "A dagger."),
            CreateItemDto("Plate Armor", ItemRarity.Uncommon, false, "Heavy armor.")
        };

        var html = _generator.Generate(items);

        html.Should().Contain("Dagger");
        html.Should().Contain("Plate Armor");
        html.Should().Contain("Common");
        html.Should().Contain("Uncommon");
    }

    [Test]
    public void Generate_FourItems_ShouldHaveNoPaddingCards()
    {
        var items = new[]
        {
            CreateItemDto("Dagger", ItemRarity.Common, false, "1"),
            CreateItemDto("Shortsword", ItemRarity.Common, false, "2"),
            CreateItemDto("Longbow", ItemRarity.Uncommon, false, "3"),
            CreateItemDto("Leather Armor", ItemRarity.Common, false, "4")
        };

        var html = _generator.Generate(items);

        int cardCount = RegexCount(html, "<div class=\"poker-card\">");
        cardCount.Should().Be(4);
    }

    [Test]
    public void Generate_ThreeItems_ShouldHaveOnePaddingCard()
    {
        var items = new[]
        {
            CreateItemDto("Dagger", ItemRarity.Common, false, "1"),
            CreateItemDto("Shortsword", ItemRarity.Common, false, "2"),
            CreateItemDto("Longbow", ItemRarity.Uncommon, false, "3")
        };

        var html = _generator.Generate(items);

        int cardCount = RegexCount(html, "<div class=\"poker-card\">");
        cardCount.Should().Be(4);
    }

    [Test]
    public void Generate_EmptyList_ShouldHaveNoCards()
    {
        var items = Array.Empty<ItemPrintDataDto>();

        var html = _generator.Generate(items);

        html.Should().Contain("<!DOCTYPE html>");
        int cardCount = RegexCount(html, "<div class=\"poker-card\">");
        cardCount.Should().Be(0);
    }

    [Test]
    public void Generate_HtmlDescription_ShouldBeEmbeddedAsIs()
    {
        var htmlDesc = "<h4>Properties</h4><ul><li>+1 AC</li><li>Resistance to fire</li></ul>";
        var items = new[]
        {
            CreateItemDto("Shield", ItemRarity.Uncommon, true, htmlDesc)
        };

        var html = _generator.Generate(items);

        html.Should().Contain("<h4>Properties</h4><ul><li>+1 AC</li><li>Resistance to fire</li></ul>");
    }

    [Test]
    public void Generate_NullDescription_ShouldRenderEmptyContent()
    {
        var items = new[]
        {
            CreateItemDto("Blank Item", ItemRarity.Undefined, false, null)
        };

        var html = _generator.Generate(items);

        html.Should().Contain("Blank Item");
        html.Should().Contain("<div class=\"card-content\"></div>");
    }

    [Test]
    public void Generate_LegendaryRarity_ShouldRenderRarityLabel()
    {
        var items = new[]
        {
            CreateItemDto("Vorpal Sword", ItemRarity.Legendary, true, "Decapitates on a natural 20.")
        };

        var html = _generator.Generate(items);

        html.Should().Contain("Legendary");
    }

    [Test]
    public void Generate_RelicsRarity_ShouldRenderRarityLabel()
    {
        var items = new[]
        {
            CreateItemDto("Godsbane", ItemRarity.Relic, true, "Divine weapon.")
        };

        var html = _generator.Generate(items);

        html.Should().Contain("Relic");
    }

    [Test]
    public void Generate_VariesRarity_ShouldRenderRarityLabel()
    {
        var items = new[]
        {
            CreateItemDto("Rod of Absorption", ItemRarity.Varies, false, "Absorbs spell levels.")
        };

        var html = _generator.Generate(items);

        html.Should().Contain("Varies");
    }

    [Test]
    public void Generate_ShouldBeValidHtmlStructure()
    {
        var items = new[]
        {
            CreateItemDto("Dagger", ItemRarity.Common, false, "A small blade.")
        };

        var html = _generator.Generate(items);

        html.Should().StartWith("<!DOCTYPE html>");
        html.Should().EndWith("</html>\r\n", "HTML should end with closing tag");
        html.Should().Contain("<head>");
        html.Should().Contain("</head>");
        html.Should().Contain("<body>");
        html.Should().Contain("</body>");
    }

    [Test]
    public void Generate_EightItems_ShouldRenderFullRows()
    {
        var items = new List<ItemPrintDataDto>();
        for (int i = 0; i < 8; i++)
            items.Add(CreateItemDto($"Item {i}", ItemRarity.Common, false, $"Desc {i}"));

        var html = _generator.Generate(items);

        int cardCount = RegexCount(html, "<div class=\"poker-card\">");
        cardCount.Should().Be(8);
    }

    [Test]
    public void Generate_OneItem_ShouldHaveThreePaddingCards()
    {
        var items = new[]
        {
            CreateItemDto("Dagger", ItemRarity.Common, false, "A dagger.")
        };

        var html = _generator.Generate(items);

        int cardCount = RegexCount(html, "<div class=\"poker-card\">");
        cardCount.Should().Be(4);
    }

    [Test]
    public void Generate_FiveItems_ShouldHaveThreePaddingCards()
    {
        var items = new[]
        {
            CreateItemDto("Item 1", ItemRarity.Common, false, "1"),
            CreateItemDto("Item 2", ItemRarity.Common, false, "2"),
            CreateItemDto("Item 3", ItemRarity.Common, false, "3"),
            CreateItemDto("Item 4", ItemRarity.Common, false, "4"),
            CreateItemDto("Item 5", ItemRarity.Common, false, "5")
        };

        var html = _generator.Generate(items);

        int cardCount = RegexCount(html, "<div class=\"poker-card\">");
        cardCount.Should().Be(8);
    }

    [Test]
    public void Generate_SpecialCharsInName_ShouldBeHtmlEncoded()
    {
        var items = new[]
        {
            CreateItemDto("Ring <of> & Protection", ItemRarity.Rare, true, "A ring.")
        };

        var html = _generator.Generate(items);

        html.Should().Contain("Ring &lt;of&gt; &amp; Protection");
    }

    [Test]
    public void Generate_WithoutAttunement_NoRareLabel_ShouldOnlyShowRarity()
    {
        var items = new[]
        {
            CreateItemDto("Dagger", ItemRarity.Common, false, "A blade.")
        };

        var html = _generator.Generate(items);

        html.Should().Contain("\"card-subtitle\">Common</div>");
    }

    static ItemPrintDataDto CreateItemDto(string name, ItemRarity rarity, bool requiresAttunement, string? description) =>
        new(name, rarity, requiresAttunement, description ?? "");

    static int RegexCount(string text, string pattern) => Regex.Matches(text, pattern).Count;
}
