using FruityScale.Domain.Enums;
using FruityScale.Domain.MusicTheory;

namespace FruityScale.Tests.Domain;

public class ChordDetectorTests
{
    [Fact]
    public void Detect_ShouldReturnNull_WhenPartitionIsEmpty()
    {
        // Arrange
        var notes = Array.Empty<int>();

        // Act
        var result = ChordDetector.Detect(notes);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Detect_ShouldReturnMelody_WhenPartitionHasOneUniqueNote()
    {
        // Arrange
        var notes = new[] { 60, 72 }; // C5, C6 (same pitch class)

        // Act
        var result = ChordDetector.Detect(notes);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.RootNote);
        Assert.Equal(ChordQuality.Melody, result.Quality);
        Assert.Equal(1, result.Weight);
        Assert.Single(result.Notes);
    }

    [Fact]
    public void Detect_ShouldDetectPowerChord()
    {
        // Arrange
        var notes = new[] { 2, 9 }; // D, A (D5 Power Chord)

        // Act
        var result = ChordDetector.Detect(notes);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.RootNote);
        Assert.Equal(ChordQuality.PowerChord, result.Quality);
        Assert.Equal(2, result.Weight);
    }

    [Fact]
    public void Detect_ShouldDetectMajorChord_InRootPosition()
    {
        // Arrange
        var notes = new[] { 0, 4, 7 }; // C, E, G (C Major)

        // Act
        var result = ChordDetector.Detect(notes);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.RootNote);
        Assert.Equal(ChordQuality.Major, result.Quality);
        Assert.Equal(3, result.Weight);
    }

    [Fact]
    public void Detect_ShouldDetectChord_RegardlessOfInversion()
    {
        // Arrange
        // E Minor: E (4), G (7), B (11)
        // 1st Inversion: G (7), B (11), E (16 - next octave)
        var notes = new[] { 7, 11, 16 }; 

        // Act
        var result = ChordDetector.Detect(notes);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.RootNote); // Root should be correctly identified as E (4)
        Assert.Equal(ChordQuality.Minor, result.Quality);
    }

    [Fact]
    public void Detect_ShouldIgnoreOctavesAndDuplicates()
    {
        // Arrange
        // F Major 7 spread across octaves with duplicates
        // F(5), A(9), C(12 -> 0), E(16 -> 4), F(17 -> 5)
        var notes = new[] { 5, 9, 12, 16, 17 }; 

        // Act
        var result = ChordDetector.Detect(notes);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.RootNote); // Root is F
        Assert.Equal(ChordQuality.Maj7, result.Quality);
        Assert.Equal(4, result.Weight);
        Assert.Equal(4, result.Notes.Count); // Should reduce to 4 unique pitch classes
    }

    [Fact]
    public void Detect_ShouldDetectComplexChords_Dom7Sharp9()
    {
        // Arrange
        // E7#9 (Jimi Hendrix chord): E (4), G# (8), B (11), D (14 -> 2), G (19 -> 7)
        var notes = new[] { 4, 8, 11, 14, 19 };

        // Act
        var result = ChordDetector.Detect(notes);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.RootNote);
        Assert.Equal(ChordQuality.Dom7Sharp9, result.Quality);
        Assert.Equal(5, result.Weight);
    }

    [Fact]
    public void Detect_ShouldReturnUnknown_WhenChordIsNotInTemplates()
    {
        // Arrange
        // A cluster of consecutive semitones
        var notes = new[] { 0, 1, 2, 3 }; 

        // Act
        var result = ChordDetector.Detect(notes);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.RootNote); // Defaults to the first unique pitch
        Assert.Equal(ChordQuality.Unknown, result.Quality);
        Assert.Equal(0, result.Weight);
    }
}
