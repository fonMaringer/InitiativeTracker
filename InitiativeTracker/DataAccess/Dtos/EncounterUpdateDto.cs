namespace InitiativeTracker.DataAccess.Dtos;

public record EncounterUpdateDto(
    int Id,
    string? Name,
    int? CurrentRound,
    int? CurrentActiveParticipantId
    );