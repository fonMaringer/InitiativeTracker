using System.ComponentModel.DataAnnotations;

namespace InitiativeTracker.Integration.RestClients.TtgClub;

public sealed class TtgClubClientOptions
{
    [Required]
    public string Host { get; set; }
    
    [Required]
    public string ApiV1Path { get; set; }
}