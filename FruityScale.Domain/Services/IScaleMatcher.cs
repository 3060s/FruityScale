using FruityScale.Domain.Models;

namespace FruityScale.Domain.Services;

public interface IScaleMatcher
{
    IEnumerable<ScaleMatchResult> Match(
        IReadOnlyList<IReadOnlyCollection<int>> scoreParts, 
        IEnumerable<ScaleDefinition> library);
}
