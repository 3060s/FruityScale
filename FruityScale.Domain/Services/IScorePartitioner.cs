using FruityScale.Domain.Models;

namespace FruityScale.Domain.Services;

public interface IScorePartitioner
{
    IReadOnlyList<IReadOnlyCollection<int>> Partition(IReadOnlyList<NoteEvent> notes);
}
