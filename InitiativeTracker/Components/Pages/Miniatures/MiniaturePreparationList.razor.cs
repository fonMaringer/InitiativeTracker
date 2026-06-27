using InitiativeTracker.Application.PrintHtmlGenerators;
using InitiativeTracker.Domain.Entities;
using Microsoft.JSInterop;

namespace InitiativeTracker.Components.Pages.Miniatures;

public partial class MiniaturePreparationList(IJSRuntime jsRuntime)
{
    private List<PrintListMiniatureEntry> _printItems = [];
    private readonly Dictionary<int, string> _images = new();
    private readonly MiniaturePrintGenerator _printGenerator = new();

    public void AddItem(Miniature miniature, int quantity)
    {
        if (quantity <= 0) return;

        var entry = _printItems.FirstOrDefault(p => p.Miniature.Id == miniature.Id);
        if (entry != null)
            entry.Quantity += quantity;
        else
        {
            _printItems.Add(new PrintListMiniatureEntry(miniature, quantity));
            if (miniature.CroppedImageData is { Length: > 0 })
                _images[miniature.Id] = $"data:image/png;base64,{Convert.ToBase64String(miniature.CroppedImageData)}";
        }

        StateHasChanged();
    }

    private void RemoveFromPrintList(int miniatureId)
    {
        _printItems.RemoveAll(p => p.Miniature.Id == miniatureId);
        StateHasChanged();
    }

    private async Task GenerateAndOpenPrintHtml()
    {
        var printDataList = new List<MiniaturePrintDataDto>();

        foreach (var printItem in _printItems)
        {
            var entity = printItem.Miniature;
            var imageBase64 = entity.CroppedImageData != null ? Convert.ToBase64String(entity.CroppedImageData) : string.Empty;

            printDataList.Add(new MiniaturePrintDataDto(
                Size: entity.Size,
                Quantity: printItem.Quantity,
                ImageBase64: imageBase64));
        }

        string html = _printGenerator.Generate(printDataList);
        await jsRuntime.InvokeVoidAsync("openHtmlInNewTab", html);
    }

    public class PrintListMiniatureEntry(Miniature miniature, int quantity)
    {
        public Miniature Miniature { get; } = miniature;
        public int Quantity { get; set; } = quantity;
    }
}
