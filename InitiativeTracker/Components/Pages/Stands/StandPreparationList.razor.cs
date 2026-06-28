using InitiativeTracker.Application.PrintHtmlGenerators;
using InitiativeTracker.Domain.Entities;
using InitiativeTracker.Domain.Enums;
using Microsoft.JSInterop;

namespace InitiativeTracker.Components.Pages.Stands;

public partial class StandPreparationList(IJSRuntime jsRuntime)
{
    private List<PrintListStandEntry> _printItems = [];
    private readonly Dictionary<int, string> _images = new();
    private readonly StandPrintGenerator _printGenerator = new();
    private readonly CreatureSize[] _sizeOptions = Enum.GetValues<CreatureSize>().Except([CreatureSize.Unknown]).ToArray();

    public void AddItem(Stand stand, int quantity)
    {
        if (quantity <= 0) return;

        _printItems.Add(new PrintListStandEntry(stand, quantity));
        if (stand.ImageData is { Length: > 0 })
            _images[stand.Id] = $"data:image/png;base64,{Convert.ToBase64String(stand.ImageData)}";

        StateHasChanged();
    }

    private void RemoveFromPrintList(PrintListStandEntry entry)
    {
        _printItems.Remove(entry);
        StateHasChanged();
    }

    private async Task GenerateAndOpenPrintHtml()
    {
        var printDataList = new List<StandPrintDataDto>();

        foreach (var printItem in _printItems)
        {
            var entity = printItem.Stand;
            var imageBase64 = entity.ImageData != null ? Convert.ToBase64String(entity.ImageData) : string.Empty;

            printDataList.Add(new StandPrintDataDto(
                Size: printItem.Size,
                Quantity: printItem.Quantity,
                ImageBase64: imageBase64,
                InverseTextColor: printItem.Stand.InverseTextColor,
                AddIndex: printItem.AddIndex,
                StartIndex: printItem.StartIndex));
        }

        string html = _printGenerator.Generate(printDataList);
        await jsRuntime.InvokeVoidAsync("openHtmlInNewTab", html);
    }

    public class PrintListStandEntry(Stand stand, int quantity)
    {
        public Stand Stand { get; } = stand;
        public CreatureSize Size { get; set; } = CreatureSize.Medium;
        public int Quantity { get; set; } = quantity;
        public bool AddIndex { get; set; } = true;
        public int StartIndex { get; set; } = 1;
    }
}