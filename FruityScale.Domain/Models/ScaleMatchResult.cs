namespace FruityScale.Domain.Models;

public record ScaleMatchResult(
    ScaleDefinition Scale,
    int RootNote,
    double Score,
    List<int>? WrongNotes,
    List<int>? MissingNotes);
