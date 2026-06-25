using InitiativeTracker.Application.PrintHtmlGenerators;
using InitiativeTracker.Domain.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace InitiativeTracker.Components.Pages.Miniatures;

public partial class MiniaturePreparationList
{
    List<PrintListMiniatureEntry> _printItems = [];
    readonly Dictionary<int, string> _images = new();
    readonly MiniaturePrintGenerator _printGenerator = new();

    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    public void AddItem(MiniatureEntity miniature, int quantity)
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

    void RemoveFromPrintList(int miniatureId)
    {
        _printItems.RemoveAll(p => p.Miniature.Id == miniatureId);
        StateHasChanged();
    }

    async Task GenerateAndOpenPrintHtml()
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
        await JSRuntime.InvokeVoidAsync("openHtmlInNewTab", html);
    }

    public class PrintListMiniatureEntry(MiniatureEntity miniature, int quantity)
    {
        public MiniatureEntity Miniature { get; } = miniature;
        public int Quantity { get; set; } = quantity;
    }
}
