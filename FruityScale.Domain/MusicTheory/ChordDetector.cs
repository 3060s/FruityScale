using FruityScale.Domain.Enums;

namespace FruityScale.Domain.MusicTheory;

public record DetectedChord(int RootNote, ChordQuality Quality, int Weight, HashSet<int> Notes);

public static class ChordDetector
{
    private static readonly Dictionary<int, ChordQuality> ChordTemplates = new()
    {
        { (1 << 0) | (1 << 7), ChordQuality.PowerChord }, // 0, 7
        
        { (1 << 0) | (1 << 4) | (1 << 7), ChordQuality.Major }, // 0, 4, 7
        { (1 << 0) | (1 << 3) | (1 << 7), ChordQuality.Minor }, // 0, 3, 7
        { (1 << 0) | (1 << 3) | (1 << 6), ChordQuality.Diminished }, // 0, 3, 6
        { (1 << 0) | (1 << 4) | (1 << 8), ChordQuality.Augmented }, // 0, 4, 8
        { (1 << 0) | (1 << 2) | (1 << 7), ChordQuality.Sus2 }, // 0, 2, 7
        { (1 << 0) | (1 << 5) | (1 << 7), ChordQuality.Sus4 }, // 0, 5, 7
        
        { (1 << 0) | (1 << 4) | (1 << 7) | (1 << 10), ChordQuality.Dom7 }, // 0, 4, 7, 10
        { (1 << 0) | (1 << 4) | (1 << 7) | (1 << 11), ChordQuality.Maj7 }, // 0, 4, 7, 11
        { (1 << 0) | (1 << 3) | (1 << 7) | (1 << 10), ChordQuality.Min7 }, // 0, 3, 7, 10
        { (1 << 0) | (1 << 3) | (1 << 6) | (1 << 10), ChordQuality.HalfDim7 }, // 0, 3, 6, 10
        { (1 << 0) | (1 << 3) | (1 << 6) | (1 << 9), ChordQuality.Dim7 }, // 0, 3, 6, 9
        { (1 << 0) | (1 << 4) | (1 << 7) | (1 << 9), ChordQuality.Maj6 }, // 0, 4, 7, 9
        { (1 << 0) | (1 << 3) | (1 << 7) | (1 << 9), ChordQuality.Min6 }, // 0, 3, 7, 9
        { (1 << 0) | (1 << 2) | (1 << 4) | (1 << 7), ChordQuality.Add9 }, // 0, 2, 4, 7
        { (1 << 0) | (1 << 2) | (1 << 3) | (1 << 7), ChordQuality.MinAdd9 }, // 0, 2, 3, 7
        { (1 << 0) | (1 << 4) | (1 << 8) | (1 << 10), ChordQuality.Aug7 }, // 0, 4, 8, 10
        { (1 << 0) | (1 << 4) | (1 << 6) | (1 << 10), ChordQuality.Dom7b5 }, // 0, 4, 6, 10
        
        { (1 << 0) | (1 << 2) | (1 << 4) | (1 << 7) | (1 << 10), ChordQuality.Dom9 }, // 0, 2, 4, 7, 10
        { (1 << 0) | (1 << 2) | (1 << 4) | (1 << 7) | (1 << 11), ChordQuality.Maj9 }, // 0, 2, 4, 7, 11
        { (1 << 0) | (1 << 2) | (1 << 3) | (1 << 7) | (1 << 10), ChordQuality.Min9 }, // 0, 2, 3, 7, 10
        { (1 << 0) | (1 << 1) | (1 << 4) | (1 << 7) | (1 << 10), ChordQuality.Dom7b9 }, // 0, 1, 4, 7, 10
        { (1 << 0) | (1 << 3) | (1 << 4) | (1 << 7) | (1 << 10), ChordQuality.Dom7Sharp9 } // 0, 3, 4, 7, 10 
    };

    public static DetectedChord? Detect(IReadOnlyCollection<int> partition)
    {
        var uniquePitches = partition.Select(n => n % MusicConstants.NotesInOctave).ToHashSet();
        
        if (uniquePitches.Count == 0) 
            return null;
        
        if (uniquePitches.Count == 1)
        {
            return new DetectedChord(uniquePitches.First(), ChordQuality.Melody, 1, uniquePitches);
        }
        
        foreach (var assumedRoot in uniquePitches)
        {
            var bitmask = 0;
            foreach (var pitch in uniquePitches)
            {
                var interval = (pitch - assumedRoot + MusicConstants.NotesInOctave) % MusicConstants.NotesInOctave;
                bitmask |= (1 << interval);
            }

            if (!ChordTemplates.TryGetValue(bitmask, out var quality)) 
                continue;
            
            var weight = (int)quality; 
            return new DetectedChord(assumedRoot, quality, weight, uniquePitches);
        }
        
        return new DetectedChord(uniquePitches.First(), ChordQuality.Unknown, 0, uniquePitches);
    }
}
