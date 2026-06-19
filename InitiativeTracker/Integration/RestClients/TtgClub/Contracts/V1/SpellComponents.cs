namespace InitiativeTracker.Integration.RestClients.TtgClub.Contracts.V1;

public class SpellComponentsSearch
{
    public bool V { get; set; }
    public bool S { get; set; }
    public bool M { get; set; }
}

public class SpellComponentsDetails
{
    public bool V { get; set; }
    public bool S { get; set; }
    public string? M { get; set; }
}