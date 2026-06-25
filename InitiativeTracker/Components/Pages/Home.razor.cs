using InitiativeTracker.Application;
using InitiativeTracker.Domain.Entities;
using InitiativeTracker.Integration.RestClients.TtgClub;
using InitiativeTracker.Integration.RestClients.TtgClub.Adapters;
using InitiativeTracker.Integration.RestClients.TtgClub.Contracts.V1;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Source = InitiativeTracker.Domain.Enums.Source;

namespace InitiativeTracker.Components.Pages;

public partial class Home : IDisposable
{
    private CancellationTokenSource _searchCts = new();
    private BestiarySearchResponseItem[]? _searchResults;
    private string? _searchPattern;
    private string _createName = string.Empty;
    private int _createHp;
    private int _createAc;

    [Inject] private IBestiaryClient BestiaryClient { get; set; } = default!;
    [Inject] private IInitiativeService InitiativeService { get; set; } = default!;

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

        _searchResults = await BestiaryClient.SearchV1Async(_searchPattern, _searchCts.Token);
    }

    private async Task Add(BestiarySearchResponseItem item, HitsMode hitsMode)
    {
        var url = item.Url;
        if (string.IsNullOrEmpty(url))
            return;

        var details = await BestiaryClient.GetDetailsV1Async(url, _searchCts.Token);

        if (details is null)
            return;

        var link = BestiaryClient.BuildDirectLink(details.Url);
        for (var i = 0; i < item.AddCount; i++)
        {
            var itemDetails = details.ToInitiativeListItem(link, hitsMode);
            InitiativeService.Append(itemDetails);
        }

        item.AddCount = 1;
    }

    private void AddManual()
    {
        var item = new InitiativeListItem
        {
            Name = _createName,
            HitsAverage = _createHp,
            ArmorClass = _createAc,
            Source = Source.Manual,
        };
        item.Initialize(HitsMode.Average);

        InitiativeService.Append(item);

        _createName = string.Empty;
        _createHp = 0;
        _createAc = 0;
    }

    public void Dispose()
    {
        _searchCts.Cancel();
        _searchCts.Dispose();
        _searchCts = new();
    }

    private static string GetItemStyle(InitiativeListItem item) => item.State switch
    {
        HealthState.Healthy => "healthy-item",
        HealthState.SlightlyWounded => "slightly-wounded-item",
        HealthState.Wounded => "wounded-item",
        HealthState.SeriouslyWounded => "seriously-wounded-item",
        HealthState.Dead => "dead-item",
        _ => throw new ArgumentOutOfRangeException(nameof(item.State), item.State, null)
    };
}
