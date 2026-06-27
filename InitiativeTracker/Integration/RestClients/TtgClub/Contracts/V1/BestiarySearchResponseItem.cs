using System.Text.Json.Serialization;

namespace InitiativeTracker.Integration.RestClients.TtgClub.Contracts.V1;

public class BestiarySearchResponseItem
{
    public Name Name { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string ChallengeRating { get; set; } = null!;
    public string Url { get; set; } = null!;
    public Source Source { get; set; } = null!;
    public Group Group { get; set; } = null!;
    
    [JsonIgnore]
    public int AddCount { get; set; } = 1;
}