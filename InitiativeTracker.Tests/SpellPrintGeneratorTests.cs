using System.Collections.Generic;
using System.Text.RegularExpressions;
using FluentAssertions;
using InitiativeTracker.Application.PrintHtmlGenerators;
using InitiativeTracker.Domain;

namespace InitiativeTracker.Tests;

public class SpellPrintGeneratorTests
{
    private readonly SpellPrintGenerator _generator = new();

    [Test]
    public void Generate_SingleSpell_ShouldReturnHtmlWithCard()
    {
        var spells = new[]
        {
            CreateSpellDto("Fireball", true, true, false, SpellClass.Wizard, "A bright streak flashes.")
        };

        var html = _generator.Generate(spells);

        html.Should().Contain("<!DOCTYPE html>");
        html.Should().Contain("\"poker-card\"");
        html.Should().Contain("Fireball");
        html.Should().Contain("\"component-badge\">V");
        html.Should().Contain("\"component-badge\">J");
        html.Should().NotContain("\"component-badge\">R");
        html.Should().Contain("Wizard");
    }

    [Test]
    public void Generate_AllComponents_ShouldRenderAllBadges()
    {
        var spells = new[]
        {
            CreateSpellDto("Lightning Bolt", true, true, true, SpellClass.Wizard, "A stroke of lightning.")
        };

        var html = _generator.Generate(spells);

        html.Should().Contain("\"component-badge\">V");
        html.Should().Contain("\"component-badge\">J");
        html.Should().Contain("\"component-badge\">R");
    }

    [Test]
    public void Generate_NoComponents_ShouldHaveNoBadges()
    {
        var spells = new[]
        {
            CreateSpellDto("Shield", false, false, false, SpellClass.Wizard, "An invisible barrier.")
        };

        var html = _generator.Generate(spells);

        html.Should().NotContain("\"component-badge\"");
    }

    [Test]
    public void Generate_MultipleSpells_ShouldRenderAllCards()
    {
        var spells = new[]
        {
            CreateSpellDto("Fireball", true, true, false, SpellClass.Wizard, "Fire."),
            CreateSpellDto("Cure Wounds", true, false, false, SpellClass.Cleric, "Healing.")
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
            CreateSpellDto("Fireball", true, true, false, SpellClass.Wizard, "1"),
            CreateSpellDto("Cure Wounds", true, false, false, SpellClass.Cleric, "2"),
            CreateSpellDto("Shield", false, false, false, SpellClass.Wizard, "3"),
            CreateSpellDto("Mage Armor", false, true, false, SpellClass.Wizard, "4")
        };

        var html = _generator.Generate(spells);

        int cardCount = RegexCount(html, "<div class=\"poker-card\">");
        cardCount.Should().Be(4);
    }

    [Test]
    public void Generate_ThreeSpells_ShouldHaveOnePaddingCard()
    {
        var spells = new[]
        {
            CreateSpellDto("Fireball", true, true, false, SpellClass.Wizard, "1"),
            CreateSpellDto("Cure Wounds", true, false, false, SpellClass.Cleric, "2"),
            CreateSpellDto("Shield", false, false, false, SpellClass.Wizard, "3")
        };

        var html = _generator.Generate(spells);

        int cardCount = RegexCount(html, "<div class=\"poker-card\">");
        cardCount.Should().Be(4);
    }

    [Test]
    public void Generate_EmptyList_ShouldHaveOnlyPaddingCards()
    {
        var spells = Array.Empty<SpellPrintDataDto>();

        var html = _generator.Generate(spells);

        html.Should().Contain("<!DOCTYPE html>");
        int cardCount = RegexCount(html, "<div class=\"poker-card\">");
        cardCount.Should().Be(0);
    }

    [Test]
    public void Generate_ShouldIncludeSpellFooter()
    {
        var spells = new[]
        {
            CreateSpellDto("Heal", true, false, true, SpellClass.Druid, "Long description.")
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
            CreateSpellDto("Firebolt", true, true, false, SpellClass.Sorcerer, htmlDesc)
        };

        var html = _generator.Generate(spells);

        html.Should().Contain("<h4>Effect</h4><p>You hurl a <strong>wand</strong> of flame.</p>");
    }

    [Test]
    public void Generate_NullDescription_ShouldRenderEmptyContent()
    {
        var spells = new[]
        {
            CreateSpellDto("Blank Spell", false, false, false, SpellClass.Bard, null)
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
            CreateSpellDto("Heroism", true, false, false, SpellClass.Barbarian, "Courage aura.")
        };

        var html = _generator.Generate(spells);

        html.Should().Contain("Barbarian");
    }

    [Test]
    public void Generate_ShouldBeValidHtmlStructure()
    {
        var spells = new[]
        {
            CreateSpellDto("Fireball", true, true, false, SpellClass.Wizard, "Fire.")
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
        var spells = new List<SpellPrintDataDto>();
        for (int i = 0; i < 8; i++)
            spells.Add(CreateSpellDto($"Spell {i}", true, false, false, SpellClass.Wizard, $"Desc {i}"));

        var html = _generator.Generate(spells);

        int cardCount = RegexCount(html, "<div class=\"poker-card\">");
        cardCount.Should().Be(8);
    }

    static SpellPrintDataDto CreateSpellDto(string name, bool verbal, bool somatic, bool material, SpellClass spellClass, string description) =>
        new(name, verbal, somatic, material, spellClass, description ?? "");

    static int RegexCount(string text, string pattern) => System.Text.RegularExpressions.Regex.Matches(text, pattern).Count;
}
