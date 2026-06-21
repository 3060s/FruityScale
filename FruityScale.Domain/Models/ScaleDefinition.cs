namespace FruityScale.Domain.Models;

public record ScaleDefinition(
    string Name,
    int[] Intervals,
    string? Description = null,
    double Popularity = 1.0);
