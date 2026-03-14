namespace InitiativeTracker.Integration.RestClients.TtgClub.Contracts.V1;

public record Search(string Value, bool Exact);
public record Order(string Field, string Direction);

public class BestiarySearchRequest
{
    public int Page { get; set; }
    public int Size { get; set; }
    public Search Search { get; set; }
    public Order[] Order { get; set; }
}
