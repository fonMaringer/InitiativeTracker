using InitiativeTracker.Domain.Entities;
using InitiativeTracker.Domain.Enums;
using InitiativeTracker.Integration.RestClients.TtgClub;
using InitiativeTracker.Integration.RestClients.TtgClub.Adapters;
using InitiativeTracker.Integration.RestClients.TtgClub.Contracts.V1;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace InitiativeTracker.Components.Pages.Encounters;

public partial class BestiarySearch(
    IBestiaryClient bestiaryClient
    ) : ComponentBase, IDisposable
{
    private CancellationTokenSource _searchCts = new();
    private BestiarySearchResponseItem[]? _searchResults;
    private string? _searchPattern;
    
    [Parameter]
    public EventCallback<EncounterParticipant> OnAddToEncounter { get; set; }

    private async Task OnKeyUpSearch(KeyboardEventArgs? e)
    {
        if (e?.Code is "Enter" or "NumpadEnter")
            await Search();
    }

    private async Task Search()
    {
        if (string.IsNullOrEmpty(_searchPattern))
        {
            _searchResults = null;
            return;
        }

        await _searchCts.CancelAsync();
        _searchCts.Token.Register(StateHasChanged);
        _searchCts.Dispose();
        _searchCts = new();

        _searchResults = await bestiaryClient.SearchV1Async(_searchPattern, _searchCts.Token);
    }

    private async Task Add(BestiarySearchResponseItem item, HitsMode hitsMode)
    {
        var url = item.Url;
        if (string.IsNullOrEmpty(url))
            return;

        var details = await bestiaryClient.GetDetailsV1Async(url, _searchCts.Token);

        if (details is null)
            return;

        var link = bestiaryClient.BuildDirectLink(details.Url);
        for (var i = 0; i < item.AddCount; i++)
        {
            var participant = details.ToEncounterParticipant(link, hitsMode);
            await OnAddToEncounter.InvokeAsync(participant);
        }

        item.AddCount = 1;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _searchCts.Cancel();
        _searchCts.Dispose();
        _searchCts = new();
    }
}