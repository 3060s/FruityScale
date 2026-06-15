namespace FruityScale.Domain.Models;

public record NoteEvent(
    int NoteNumber,
    string Name,
    string Key);