using FruityScale.Domain.Enums;
using FruityScale.Domain.Models;
using FruityScale.Domain.Services;

namespace FruityScale.Domain.MusicTheory;

public class ScaleMatcher : IScaleMatcher
{
    public IEnumerable<ScaleMatchResult> Match(
        IReadOnlyList<IReadOnlyCollection<int>> scoreParts, 
        IEnumerable<ScaleDefinition> library)
    {
        var allUniqueNotes = scoreParts
            .SelectMany(p => p)
            .Select(n => n % MusicConstants.NotesInOctave)
            .ToHashSet();

        if (allUniqueNotes.Count == 0) return [];
        
        var progression = scoreParts
            .Select(ChordDetector.Detect)
            .Where(chord => chord != null)
            .ToList();

        var chordsOnly = progression.Where(c => c.Quality >= ChordQuality.PowerChord).ToList();
        var results = new List<ScaleMatchResult>();

        foreach (var scale in library)
        {
            for (int rootNote = 0; rootNote < MusicConstants.NotesInOctave; rootNote++)
            {
                var scaleNotes = scale.Intervals
                    .Select(interval => (interval + rootNote) % MusicConstants.NotesInOctave)
                    .ToHashSet();
                
                var matches = allUniqueNotes.Intersect(scaleNotes).ToList();
                double baseScore = (double)matches.Count / allUniqueNotes.Count;

                if (!(baseScore > 0)) 
                    continue;

                double heuristicBonus = 0;
    
                foreach (var chord in progression)
                {
                    if (chord.Notes.IsSubsetOf(scaleNotes))
                    {
                        double normalizedChordWeight = Math.Clamp(chord.Weight * 0.05, 0.1, 0.3);
                        heuristicBonus += normalizedChordWeight; 
                    }
                }
    
                if (chordsOnly.Count != 0)
                {
                    var firstChord = chordsOnly.First();
                    var lastChord = chordsOnly.Last();
        
                    if (firstChord.RootNote == rootNote) heuristicBonus += 0.5;
        
                    if (lastChord.RootNote == rootNote) heuristicBonus += 1.0; 
                }
                
                double popularityBonus = scale.Popularity * 0.5;
                double finalScore = baseScore + heuristicBonus + popularityBonus;

                results.Add(new ScaleMatchResult(
                    Scale: scale,
                    RootNote: rootNote,
                    Score: finalScore,
                    WrongNotes: allUniqueNotes.Except(scaleNotes).ToList(),
                    MissingNotes: scaleNotes.Except(allUniqueNotes).ToList()
                ));
            }
        }
        
        return results.OrderByDescending(r => r.Score).ToList();
    }
}
