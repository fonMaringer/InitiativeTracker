using InitiativeTracker.DataAccess.Dtos;
using InitiativeTracker.DataAccess.Repositories;
using InitiativeTracker.Domain.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace InitiativeTracker.Components.Pages.Stands;

public partial class AddStandForm(
    IStandRepository standRepository,
    IJSRuntime jsRuntime
    )
{
    private byte[]? _imageData;
    private bool _inverseTextColor;
    private bool _isProcessing;
    private string? _errorMessage;
    private string? _successMessage;

    private bool _isDisabled => _isProcessing || (_editStand == null && _imageData == null);

    [Parameter]
    public EventCallback OnDataChanged { get; set; }
    [Parameter]
    public Stand? EditStand { get; set; }
    [Parameter]
    public EventCallback<Stand?> OnEditStandChanged { get; set; }

    private Stand? _editStand;

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        OnParametersSet();

        if (_editStand != null && _imageData is null)
        {
            var imageBytes = await standRepository.GetImageAsync(_editStand.Id);
            if (imageBytes.Length > 0)
            {
                _imageData = imageBytes;
            }

            _inverseTextColor = _editStand.InverseTextColor;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var dotNetHelper = DotNetObjectReference.Create(this);
            await jsRuntime.InvokeVoidAsync("registerPasteListener", "addStandForm", dotNetHelper);
        }
    }

    [JSInvokable]
    public void HandleClipboardImage(string mimeType, string base64)
    {
        try
        {
            _imageData = Convert.FromBase64String(base64);
            _errorMessage = null;
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to read pasted image: {ex.Message}";
        }

        InvokeAsync(StateHasChanged);
    }

    protected override void OnParametersSet()
    {
        _editStand = EditStand;
        if (_editStand != null)
        {
            _errorMessage = null;
            _successMessage = null;
        }
        else
        {
            ResetForm();
        }
    }

    private async Task OnImageSelected(InputFileChangeEventArgs e)
    {
        var firstFile = e.File;

        await using var stream = firstFile.OpenReadStream();
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        _imageData = ms.ToArray();
        _errorMessage = null;
    }

    private async Task OnSave()
    {
        _isProcessing = true;
        _errorMessage = null;
        _successMessage = null;

        try
        {
            if (_editStand != null)
            {
                var dto = new StandUpdateDto(
                    ImageData: _imageData!,
                    InverseTextColor: _inverseTextColor);
                await standRepository.UpdateAsync(_editStand.Id, dto);

                _successMessage = $"Stand updated successfully.";
                await OnEditStandChanged.InvokeAsync(null);
                await OnDataChanged.InvokeAsync();
            }
            else
            {
                await standRepository.AddAsync(new StandCreateDto(
                    ImageData: _imageData!,
                    InverseTextColor: _inverseTextColor));

                _successMessage = $"Stand added successfully.";
                ResetForm();
                await OnDataChanged.InvokeAsync();
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to save stand: {ex.Message}";
        }
        finally
        {
            _isProcessing = false;
        }
    }

    private async Task OnCancel()
    {
        await OnEditStandChanged.InvokeAsync(null);
    }

    private void ResetForm()
    {
        _imageData = null;
        _inverseTextColor = false;
    }
}
