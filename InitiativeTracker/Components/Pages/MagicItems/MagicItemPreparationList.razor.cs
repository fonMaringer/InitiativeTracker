using InitiativeTracker.Application.PrintHtmlGenerators;
using InitiativeTracker.Domain.Entities;
using Microsoft.JSInterop;

namespace InitiativeTracker.Components.Pages.MagicItems;

public partial class MagicItemPreparationList(IJSRuntime jsRuntime)
{
    private readonly List<PrintListItemEntry> _printItems = [];
    private readonly PokerCardPrintGenerator _printGenerator = new();

    public void AddItem(MagicItem item, int quantity)
    {
        if (quantity <= 0) return;

        var entry = _printItems.FirstOrDefault(p => p.Item.Id == item.Id);
        if (entry != null)
            entry.Quantity += quantity;
        else
            _printItems.Add(new PrintListItemEntry(item, quantity));

        StateHasChanged();
    }

    private void RemoveFromPrintList(int itemId)
    {
        _printItems.RemoveAll(p => p.Item.Id == itemId);
        StateHasChanged();
    }

    private async Task GenerateAndOpenPrintHtml()
    {
        var printDataList = new List<PokerCardPrintDataDto>();

        foreach (var printItem in _printItems)
        {
            var entity = printItem.Item;

            for (var n = 0; n < printItem.Quantity; n++)
            {
                var subtitle = string.IsNullOrEmpty(printItem.Item.Type)
                    ? printItem.Item.Rarity.ToString()
                    : $"{printItem.Item.Type}, {printItem.Item.Rarity}";

                printDataList.Add(new PokerCardPrintDataDto(
                    entity.Name,
                    subtitle,
                    entity.RequiresAttunement ? ["A"] : [],
                    [],
                    entity.Description,
                    null,
                    null));
            }
        }

        var html = _printGenerator.Generate(printDataList);
        await jsRuntime.InvokeVoidAsync("openHtmlInNewTab", html);
    }

    public class PrintListItemEntry(MagicItem item, int quantity)
    {
        public MagicItem Item { get; } = item;
        public int Quantity { get; set; } = quantity;
    }
}
