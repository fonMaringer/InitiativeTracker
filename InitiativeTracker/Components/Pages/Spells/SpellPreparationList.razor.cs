using InitiativeTracker.Application.PrintHtmlGenerators;
using InitiativeTracker.Domain.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace InitiativeTracker.Components.Pages.Spells;

public partial class SpellPreparationList
{
    readonly List<PrintListSpellEntry> _printItems = [];
    readonly PokerCardPrintGenerator _printGenerator = new();

    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    public void AddItem(SpellEntity spell, int quantity)
    {
        if (quantity <= 0) return;

        var entry = _printItems.FirstOrDefault(p => p.Spell.Id == spell.Id);
        if (entry != null)
            entry.Quantity += quantity;
        else
            _printItems.Add(new PrintListSpellEntry(spell, quantity));

        StateHasChanged();
    }

    void RemoveFromPrintList(int spellId)
    {
        _printItems.RemoveAll(p => p.Spell.Id == spellId);
        StateHasChanged();
    }

    async Task GenerateAndOpenPrintHtml()
    {
        var printDataList = new List<PokerCardPrintDataDto>();

        foreach (var printItem in _printItems)
        {
            var entity = printItem.Spell;

            for (var n = 0; n < printItem.Quantity; n++)
            {
                var flags = new List<string>();
                if (entity.Concentration)
                    flags.Add("C");

                var components = new List<string>();
                if (entity.VerbalComponent) components.Add("<b>V</b>");
                if (entity.SomaticComponent) components.Add("<b>S</b>");
                if (!string.IsNullOrEmpty(entity.MaterialComponent)) components.Add($"<b>M</b> ({entity.MaterialComponent})");
                var additionalInfo = new[]
                {
                    $"<b>Time:</b> {entity.Time}",
                    $"<b>Distance:</b> {entity.Range}",
                    $"<b>Components:</b> {string.Join(", ", components)}",
                    $"<b>Duration:</b> {entity.Duration}",
                };
                printDataList.Add(
                    new PokerCardPrintDataDto(
                        entity.Name,
                        $"{entity.LevelDescription}, {entity.Type}",
                        flags,
                        additionalInfo,
                        entity.Description,
                        entity.Upper,
                        string.Join(", ", entity.Classes)));
            }
        }

        var html = _printGenerator.Generate(printDataList);
        await JsRuntime.InvokeVoidAsync("openHtmlInNewTab", html);
    }

    public class PrintListSpellEntry(SpellEntity spell, int quantity)
    {
        public SpellEntity Spell { get; } = spell;
        public int Quantity { get; set; } = quantity;
    }
}
