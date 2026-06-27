using System.Text.RegularExpressions;
using FluentAssertions;
using InitiativeTracker.Application.PrintHtmlGenerators;

namespace InitiativeTracker.Tests.PrintGeneratorTests;

public class SpellPrintGeneratorTests
{
    private readonly PokerCardPrintGenerator _generator = new();

    [Test]
    public void Generate_SingleSpell_ShouldReturnHtmlWithCard()
    {
        var spells = new[]
        {
            CreateSpellDto("Fireball", "Type", true, true, null, "Wizard", "A bright streak flashes.")
        };

        var html = _generator.Generate(spells);

        html.Should().Contain("<!DOCTYPE html>");
        html.Should().Contain("\"poker-card\"");
        html.Should().Contain("Fireball");
        html.Should().Contain("\"flag-badge\">V");
        html.Should().Contain("\"flag-badge\">S");
        html.Should().NotContain("\"flag-badge\">M");
        html.Should().Contain("Wizard");
    }

    [Test]
    public void Generate_AllComponents_ShouldRenderAllBadges()
    {
        var spells = new[]
        {
            CreateSpellDto("Lightning Bolt", "Type", true, true, "Some", null!, "A stroke of lightning.")
        };

        var html = _generator.Generate(spells);

        html.Should().Contain("\"flag-badge\">V");
        html.Should().Contain("\"flag-badge\">S");
        html.Should().Contain("\"flag-badge\">M");
    }

    [Test]
    public void Generate_NoComponents_ShouldHaveNoBadges()
    {
        var spells = new[]
        {
            CreateSpellDto("Shield", "Type", false, false, null, null!, "An invisible barrier.")
        };

        var html = _generator.Generate(spells);

        html.Should().NotContain("\"flag-badge\"");
    }

    [Test]
    public void Generate_MultipleSpells_ShouldRenderAllCards()
    {
        var spells = new[]
        {
            CreateSpellDto("Fireball", "Type", true, true, null, "Wizard", "Fire."),
            CreateSpellDto("Cure Wounds", "Type", true, false, null, "Cleric", "Healing.")
        };

        var html = _generator.Generate(spells);

        html.Should().Contain("Fireball");
        html.Should().Contain("Cure Wounds");
        html.Should().Contain("Wizard");
        html.Should().Contain("Cleric");
    }

    [Test]
    public void Generate_FourSpells_ShouldHaveNoPaddingCards()
    {
        var spells = new[]
        {
            CreateSpellDto("Fireball", "Type", true, true, null, null!, "1"),
            CreateSpellDto("Cure Wounds", "Type", true, false, null, "Some", "2"),
            CreateSpellDto("Shield", "Type", false, false, null, null!, "3"),
            CreateSpellDto("Mage Armor", "Type", false, true, null, null!, "4")
        };

        var html = _generator.Generate(spells);

        var cardCount = RegexCount(html, "<div class=\"poker-card\">");
        cardCount.Should().Be(4);
    }

    [Test]
    public void Generate_ThreeSpells_ShouldHaveOnePaddingCard()
    {
        var spells = new[]
        {
            CreateSpellDto("Fireball", "Type", true, true, null, null!, "1"),
            CreateSpellDto("Cure Wounds", "Type", true, false, null, "Some", "2"),
            CreateSpellDto("Shield", "Type", false, false, null, null!, "3")
        };

        var html = _generator.Generate(spells);

        var cardCount = RegexCount(html, "<div class=\"poker-card\">");
        cardCount.Should().Be(3);
    }

    [Test]
    public void Generate_EmptyList_ShouldHaveOnlyPaddingCards()
    {
        var spells = Array.Empty<PokerCardPrintDataDto>();

        var html = _generator.Generate(spells);

        html.Should().Contain("<!DOCTYPE html>");
        var cardCount = RegexCount(html, "<div class=\"poker-card\">");
        cardCount.Should().Be(0);
    }

    [Test]
    public void Generate_ShouldIncludeSpellFooter()
    {
        var spells = new[]
        {
            CreateSpellDto("Heal", "Type", true, false, "Some", "Druid", "Long description.")
        };

        var html = _generator.Generate(spells);

        html.Should().Contain("\"card-footer\"");
        html.Should().Contain("Druid");
    }

    [Test]
    public void Generate_HtmlDescription_ShouldBeEmbeddedAsIs()
    {
        var htmlDesc = "<h4>Effect</h4><p>You hurl a <strong>wand</strong> of flame.</p>";
        var spells = new[]
        {
            CreateSpellDto("Firebolt", "Type", true, true, null, null!, htmlDesc)
        };

        var html = _generator.Generate(spells);

        html.Should().Contain("<h4>Effect</h4><p>You hurl a <strong>wand</strong> of flame.</p>");
    }

    [Test]
    public void Generate_NullDescription_ShouldRenderEmptyContent()
    {
        var spells = new[]
        {
            CreateSpellDto("Blank Spell", "Type", false, false, null, null!, null!)
        };

        var html = _generator.Generate(spells);

        html.Should().Contain("Blank Spell");
        html.Should().Contain("<div class=\"card-content\"></div>");
    }

    [Test]
    public void Generate_BarbarianClass_ShouldRenderClassName()
    {
        var spells = new[]
        {
            CreateSpellDto("Heroism", "Type", true, false, null, "Barbarian", "Courage aura.")
        };

        var html = _generator.Generate(spells);

        html.Should().Contain("Barbarian");
    }

    [Test]
    public void Generate_ShouldBeValidHtmlStructure()
    {
        var spells = new[]
        {
            CreateSpellDto("Fireball", "Type", true, true, null, null!, "Fire.")
        };

        var html = _generator.Generate(spells);

        html.Should().StartWith("<!DOCTYPE html>");
        html.Should().EndWith("</html>\r\n", "HTML should end with closing tag");
        html.Should().Contain("<head>");
        html.Should().Contain("</head>");
        html.Should().Contain("<body>");
        html.Should().Contain("</body>");
    }

    [Test]
    public void Generate_EightSpells_ShouldRenderFullRows()
    {
        var spells = new List<PokerCardPrintDataDto>();
        for (var i = 0; i < 8; i++)
            spells.Add(CreateSpellDto($"Spell {i}", "Type", true, false, null, null!, $"Desc {i}"));

        var html = _generator.Generate(spells);

        var cardCount = RegexCount(html, "<div class=\"poker-card\">");
        cardCount.Should().Be(8);
    }

    private static PokerCardPrintDataDto CreateSpellDto(string name, string type, bool verbal, bool somatic, string? material, string spellClass, string description)
    {
        var flags = new List<string>();
        if (verbal) flags.Add("V");
        if (somatic) flags.Add("S");
        if (!string.IsNullOrEmpty(material)) flags.Add("M");
        return new(name, type, flags, [], description ?? "", null, spellClass);
    }

    private static int RegexCount(string text, string pattern) => Regex.Matches(text, pattern).Count;
}
