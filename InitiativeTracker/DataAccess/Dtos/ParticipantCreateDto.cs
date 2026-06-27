namespace InitiativeTracker.DataAccess.Dtos;

public record ParticipantCreateDto(
    string Name,
    int Hp,
    int Ac,
    int Dexterity
);