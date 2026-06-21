using FruityScale.Domain.Models;
using FruityScale.Domain.Services;

namespace FruityScale.Domain.MusicTheory;

public class ScorePartitioner : IScorePartitioner
{
    public IReadOnlyList<IReadOnlyCollection<int>> Partition(IReadOnlyList<NoteEvent> notes)
    {
        if (!notes.Any()) 
            return [];

        var pianoRollLength = notes.Max(ne => ne.Time + ne.Length);
        var partitions = new List<IReadOnlyCollection<int>>();
        
        foreach (var i in Enumerable.Range(0, pianoRollLength))
        {
            var currentNoteNumbers = notes
                .Where(ne => ne.Time <= i && (ne.Time + ne.Length) > i)
                .Select(ne => ne.NoteNumber)
                .ToList();

            var lastPartition = partitions.LastOrDefault();
            
            if (lastPartition == null || 
                currentNoteNumbers.Count != lastPartition.Count || 
                currentNoteNumbers.Except(lastPartition).Any())
            {
                partitions.Add(currentNoteNumbers);
            }
        }

        return partitions;
    }
}
