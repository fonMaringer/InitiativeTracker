using InitiativeTracker.DataAccess.Repositories;
using InitiativeTracker.Domain.Entities;
using Microsoft.AspNetCore.Components;

namespace InitiativeTracker.Components.Pages.Stands;

public partial class StandCatalog(IStandRepository standRepository)
{
    private List<Stand> _stands = [];
    private bool _isLoading;

    [Parameter]
    public EventCallback<Stand?> OnEditSelected { get; set; }
    [Parameter]
    public EventCallback OnDataChanged { get; set; }
    [Parameter]
    public EventCallback<Stand> OnAddForPrint { get; set; }

    protected override async Task OnInitializedAsync() => await LoadAllStands();

    private async Task LoadAllStands()
    {
        _isLoading = true;
        StateHasChanged();
        try
        {
            var allItems = await standRepository.GetAllAsync();
            _stands = new List<Stand>(allItems);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load catalog: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task SelectForEdit(Stand? stand) => await OnEditSelected.InvokeAsync(stand);

    private async Task OnAddToPrint(Stand? stand) => await OnAddForPrint.InvokeAsync(stand!);

    private async Task OnDelete(Stand stand)
    {
        try
        {
            await standRepository.DeleteAsync(stand.Id);
            _stands.Remove(stand);
            StateHasChanged();
            await OnDataChanged.InvokeAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to delete stand: {ex.Message}");
        }
    }
}
