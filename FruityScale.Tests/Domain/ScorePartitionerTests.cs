using FruityScale.Domain.Models;
using FruityScale.Domain.MusicTheory;

namespace FruityScale.Tests.Domain;

public class ScorePartitionerTests
{
    private readonly ScorePartitioner _sut;

    public ScorePartitionerTests()
    {
        _sut = new ScorePartitioner();
    }

    [Fact]
    public void Partition_ShouldReturnEmpty_WhenNotesListIsEmpty()
    {
        // Arrange
        var emptyNotes = new List<NoteEvent>();

        // Act
        var result = _sut.Partition(emptyNotes);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Partition_ShouldCreateSinglePartition_WhenNotesOverlapPerfectly()
    {
        // Arrange
        var notes = new List<NoteEvent>
        {
            new NoteEvent(60, "C", "C5", 0, 4),
            new NoteEvent(64, "E", "E5", 0, 4),
            new NoteEvent(67, "G", "G5", 0, 4)
        };

        // Act
        var result = _sut.Partition(notes);

        // Assert
        Assert.Single(result);
        Assert.Contains(60, result[0]);
        Assert.Contains(64, result[0]);
        Assert.Contains(67, result[0]);
        Assert.Equal(3, result[0].Count);
    }

    [Fact]
    public void Partition_ShouldCreateSeparatePartitions_WhenNotesAreSequential()
    {
        // Arrange
        var notes = new List<NoteEvent>
        {
            new NoteEvent(60, "C", "C5", 0, 2),
            new NoteEvent(62, "D", "D5", 2, 2),
            new NoteEvent(64, "E", "E5", 4, 2)
        };

        // Act
        var result = _sut.Partition(notes).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(new[] { 60 }, result[0]);
        Assert.Equal(new[] { 62 }, result[1]);
        Assert.Equal(new[] { 64 }, result[2]);
    }

    [Fact]
    public void Partition_ShouldCreateNewPartition_WhenNewNoteIsAddedToSustainedNote()
    {
        // Arrange
        var notes = new List<NoteEvent>
        {
            new NoteEvent(60, "C", "C5", 0, 8), // Sustained note
            new NoteEvent(64, "E", "E5", 4, 4)  // Note joining halfway
        };

        // Act
        var result = _sut.Partition(notes).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        
        // First partition: only the sustained note
        Assert.Single(result[0]);
        Assert.Contains(60, result[0]);
        
        // Second partition: both notes playing together
        Assert.Equal(2, result[1].Count);
        Assert.Contains(60, result[1]);
        Assert.Contains(64, result[1]);
    }

    [Fact]
    public void Partition_ShouldCreateNewPartition_WhenNoteEndsWhileOtherSustains()
    {
        // Arrange
        var notes = new List<NoteEvent>
        {
            new NoteEvent(60, "C", "C5", 0, 8), 
            new NoteEvent(64, "E", "E5", 0, 4)  
        };

        // Act
        var result = _sut.Partition(notes).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        
        Assert.Equal(2, result[0].Count);
        Assert.Contains(60, result[0]);
        Assert.Contains(64, result[0]);
        
        Assert.Single(result[1]);
        Assert.Contains(60, result[1]);
    }

    [Fact]
    public void Partition_ShouldHaveGaps()
    {
        // Arrange
        var notes = new List<NoteEvent>
        {
            new NoteEvent(60, "C", "C5", 0, 2),
            // Gap between time 2 and 4
            new NoteEvent(62, "D", "D5", 4, 2)
        };

        // Act
        var result = _sut.Partition(notes).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(new[] { 60 }, result[0]);
        Assert.Equal([], result[1]);
        Assert.Equal(new[] { 62 }, result[2]);
    }
}
