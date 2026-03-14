using System.Net.Http.Headers;
using System.Text.Json;
using InitiativeTracker.Integration.RestClients.TtgClub.Contracts.V1;
using Microsoft.Extensions.Options;

namespace InitiativeTracker.Integration.RestClients.TtgClub;

public interface IBestiaryClient
{
    Task<BestiarySearchResponseItem[]> SearchV1Async(string query, CancellationToken ct);
    
    Task<string> GetDetailsV1Async(string url, CancellationToken ct);
}

public class BestiaryClient(IOptions<TtgClubClientOptions> options) : IBestiaryClient
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
    
    private const string MediaType = "application/json";
    private readonly string _apiV1Path = options.Value.ApiV1Path;
    
    private readonly HttpClient _bestiaryV1Client = new()
    {
        BaseAddress = new Uri(options.Value.Host),
        DefaultRequestHeaders =
        {
            { "Accept", MediaType },
        },
    };

    public async Task<BestiarySearchResponseItem[]> SearchV1Async(string query, CancellationToken ct)
    {
        var request = new BestiarySearchRequest
        {
            Page = 0,
            Size = 100,
            Search = new(query, false),
            Order =
            [
                new("exp", "asc"),
                new("name", "asc"),
            ],
        };

        var stringRequest = JsonSerializer.Serialize(request, _jsonSerializerOptions);

        var content = new StringContent(stringRequest, mediaType: new MediaTypeHeaderValue(MediaType));

        var response = await _bestiaryV1Client.PostAsync(_apiV1Path + "/bestiary", content, ct);

        var responseItems = await response.Content.ReadFromJsonAsync<BestiarySearchResponseItem[]>(ct);

        return responseItems ?? [];
    }

    public async Task<string> GetDetailsV1Async(string url, CancellationToken ct)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, _apiV1Path + url);
        
        var response = await _bestiaryV1Client.SendAsync(requestMessage, ct);
        
        //TODO: add contract here
        var responseString = await response.Content.ReadAsStringAsync(ct);

        return responseString;
    }
}
