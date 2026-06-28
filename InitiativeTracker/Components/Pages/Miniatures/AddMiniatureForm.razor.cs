using Cropper.Blazor.Components;
using Cropper.Blazor.Events;
using Cropper.Blazor.Events.CropEndEvent;
using Cropper.Blazor.Events.CropReadyEvent;
using Cropper.Blazor.Models;
using InitiativeTracker.DataAccess.Dtos;
using InitiativeTracker.DataAccess.Repositories;
using InitiativeTracker.Domain.Entities;
using InitiativeTracker.Domain.Enums;
using InitiativeTracker.Integration.RestClients.TtgClub;
using InitiativeTracker.Integration.RestClients.TtgClub.Contracts.V1;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace InitiativeTracker.Components.Pages.Miniatures;

public partial class AddMiniatureForm(
    IMiniatureRepository miniatureRepository,
    IJSRuntime jsRuntime,
    IBestiaryClient bestiaryClient
    ) : IDisposable
{
    private string _name = string.Empty;
    private string? _link;
    private CreatureSize _size = CreatureSize.Medium;
    private byte[]? _imageData;
    private string? _imageDataSrc;
    private bool _isProcessing;
    private string? _errorMessage;
    private string? _successMessage;
    private int? _printedCount = 0;

    private double _cropX;
    private double _cropY;
    private double _cropWidth;
    private double _cropHeight;

    private CropperComponent? _cropperComponent;

    private readonly Options CropperOptions = new()
    {
        AspectRatio = 25m / 32m,
        ViewMode = ViewMode.Vm0,
        AutoCropArea = 1m,
        Scalable = true,
        CropBoxResizable = true,
    };

    private readonly CreatureSize[] _sizeOptions = Enum.GetValues<CreatureSize>().Except([CreatureSize.Unknown]).ToArray();
    private bool _isDisabled => _isProcessing || string.IsNullOrWhiteSpace(_name) || (_editMiniature == null && _imageData == null);

    [Parameter]
    public EventCallback OnDataChanged { get; set; }
    [Parameter]
    public Miniature? EditMiniature { get; set; }
    [Parameter]
    public EventCallback<Miniature?> OnEditMiniatureChanged { get; set; }

    private Miniature? _editMiniature;

    public void Dispose()
    {
        _searchCts.Cancel();
        _searchCts.Dispose();
    }

    private string _searchPattern = string.Empty;
    private BestiarySearchResponseItem[]? _searchResults;
    private CancellationTokenSource _searchCts = new();

    private async Task OnKeyUpSearch(KeyboardEventArgs? e)
    {
        if (e?.Code is "Enter" or "NumpadEnter")
        {
            await SearchBestiary();
        }
    }

    private async Task SearchBestiary()
    {
        _searchResults = null;
        StateHasChanged();

        if (string.IsNullOrEmpty(_searchPattern))
        {
            return;
        }

        await _searchCts.CancelAsync();
        _searchCts.Token.Register(StateHasChanged);
        _searchCts.Dispose();
        _searchCts = new();

        try
        {
            _searchResults = await bestiaryClient.SearchV1Async(_searchPattern, _searchCts.Token);
        }
        catch (OperationCanceledException)
        {
            // Ignored due to re-search throttling
        }
    }

    private async Task SelectMiniature(BestiarySearchResponseItem item)
    {
        var url = item.Url;
        if (string.IsNullOrEmpty(url))
        {
            return;
        }

        try
        {
            var details = await bestiaryClient.GetDetailsV1Async(url, _searchCts.Token);

            if (details is null)
                return;

            var link = bestiaryClient.BuildDirectLink(item.Url);

            _name = details.Name.Rus;
            _link = link;
            _size = details.Size.CreatureSize;

            var imageUrl = details.Images.FirstOrDefault(img => !img.Contains("tokens", StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(imageUrl))
            {
                await DownloadImageAsync(imageUrl);
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to load bestiary details: {ex.Message}";
        }
    }

    private async Task DownloadImageAsync(string imageUrl)
    {
        try
        {
            using var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30),
            };
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");

            _imageData = await httpClient.GetByteArrayAsync(imageUrl);
            _imageDataSrc = $"data:image/jpeg;base64,{Convert.ToBase64String(_imageData)}";
            _errorMessage = null;
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to download image: {ex.Message}";
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        OnParametersSet();

        if (_editMiniature != null && string.IsNullOrEmpty(_imageDataSrc))
        {
            var imageBytes = await miniatureRepository.GetImageAsync(_editMiniature.Id);
            if (imageBytes.Length > 0)
            {
                _imageData = imageBytes;
                _imageDataSrc = $"data:image/jpeg;base64,{Convert.ToBase64String(imageBytes)}";
            }
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var dotNetHelper = DotNetObjectReference.Create(this);
            await jsRuntime.InvokeVoidAsync("registerPasteListener", "addMiniatureForm", dotNetHelper);
        }
    }

    [JSInvokable]
    public void HandleClipboardImage(string mimeType, string base64)
    {
        try
        {
            _imageData = Convert.FromBase64String(base64);
            _imageDataSrc = $"data:{mimeType};base64,{base64}";
            _errorMessage = null;
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to read pasted image: {ex.Message}";
        }

        InvokeAsync(StateHasChanged);
    }

    private void OnCropperReady(JSEventData<CropReadyEvent>? data)
    {
        _ = HandleCropperReadyAsync();
    }

    private async Task HandleCropperReadyAsync()
    {
        if (_cropperComponent == null) return;

        if (_editMiniature != null)
        {
            await RestoreCropRegionToCropper();
        }

        await InvokeAsync(StateHasChanged);
    }

    private void OnCropEnd(JSEventData<CropEndEvent>? data)
    {
        _ = HandleCropEndAsync();
    }

    private async Task HandleCropEndAsync()
    {
        if (_cropperComponent == null) return;

        var cropperData = await _cropperComponent.GetDataAsync(true);
        _cropX = (double)(cropperData.X ?? 0m);
        _cropY = (double)(cropperData.Y ?? 0m);
        _cropWidth = (double)(cropperData.Width ?? 0m);
        _cropHeight = (double)(cropperData.Height ?? 0m);
        await InvokeAsync(StateHasChanged);
    }

    protected override void OnParametersSet()
    {
        _editMiniature = EditMiniature;
        if (_editMiniature != null)
        {
            _name = _editMiniature.Name;
            _link = _editMiniature.Link;
            _size = _editMiniature.Size;
            _errorMessage = null;
            _successMessage = null;
            _printedCount = _editMiniature.PrintedCount;

            _cropX = _editMiniature.CropX;
            _cropY = _editMiniature.CropY;
            _cropWidth = _editMiniature.CropWidth;
            _cropHeight = _editMiniature.CropHeight;
        }
        else
        {
            ResetForm();
        }
    }

    private Task RestoreCropRegionToCropper()
    {
        _cropperComponent!.SetData(new SetDataOptions
        {
            X = (decimal)_cropX,
            Y = (decimal)_cropY,
            Width = (decimal)_cropWidth,
            Height = (decimal)_cropHeight,
        });

        return Task.CompletedTask;
    }

    private async Task OnImageSelected(InputFileChangeEventArgs e)
    {
        var firstFile = e.File;

        await using var stream = firstFile.OpenReadStream();
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        _imageData = ms.ToArray();
        _imageDataSrc = $"data:{firstFile.ContentType};base64,{Convert.ToBase64String(_imageData)}";
        _errorMessage = null;
    }

    private async Task OnSave()
    {
        if (_cropperComponent == null)
            return;
        
        _isProcessing = true;
        _errorMessage = null;
        _successMessage = null;

        try
        {
            var options = new GetCroppedCanvasOptions
            {
                FillColor = "#ffffff",
                ImageSmoothingEnabled = true,
            };

#pragma warning disable CS0618 // Type or member is obsolete
            var nativeCroppedBase64 = await _cropperComponent.GetCroppedCanvasDataURLAsync(options, "image/jpeg", 0.85f);
#pragma warning restore CS0618 // Type or member is obsolete

            var cleanBase64 = nativeCroppedBase64.Contains(",") ? nativeCroppedBase64.Split(',')[1] : nativeCroppedBase64;
            var croppedImageData = Convert.FromBase64String(cleanBase64);

            if (_editMiniature != null)
            {
                var dto = new MiniatureUpdateDto(
                    Name: _name,
                    Size: _size,
                    ImageData: _imageData!,
                    CroppedImageData: croppedImageData,
                    PrintedCount: _printedCount,
                    Link: _link,
                    CropX: _cropX,
                    CropY: _cropY,
                    CropWidth: _cropWidth,
                    CropHeight: _cropHeight);
                await miniatureRepository.UpdateAsync(_editMiniature.Id, dto);

                _successMessage = $"'{_name}' updated successfully.";
                await OnEditMiniatureChanged.InvokeAsync(null);
                await OnDataChanged.InvokeAsync();
            }
            else
            {
                await miniatureRepository.AddAsync(new MiniatureCreateDto(
                    Name: _name,
                    Size: _size,
                    ImageData: _imageData!,
                    CroppedImageData: croppedImageData ?? [],
                    PrintedCount: _printedCount ?? 0,
                    Link: _link,
                    CropX: _cropX,
                    CropY: _cropY,
                    CropWidth: _cropWidth,
                    CropHeight: _cropHeight));

                _successMessage = $"'{_name}' added successfully.";
                ResetForm();
                await OnDataChanged.InvokeAsync();
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to save miniature: {ex.Message}";
        }
        finally
        {
            _isProcessing = false;
        }
    }

    private async Task OnCancel()
    {
        await OnEditMiniatureChanged.InvokeAsync(null);
    }

    private void ResetForm()
    {
        _name = string.Empty;
        _size = CreatureSize.Medium;
        _imageData = null;
        _printedCount = null;
        _link = null;
        _imageDataSrc = null;
        _cropX = 0;
        _cropY = 0;
        _cropWidth = 0;
        _cropHeight = 0;
    }
}
