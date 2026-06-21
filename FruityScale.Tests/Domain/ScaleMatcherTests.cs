using FruityScale.Domain.Models;
using FruityScale.Domain.MusicTheory;

namespace FruityScale.Tests.Domain;

public class ScaleMatcherTests
{
    private readonly ScaleMatcher _sut;
    private readonly List<ScaleDefinition> _mockLibrary;

    public ScaleMatcherTests()
    {
        _sut = new ScaleMatcher();
        
        _mockLibrary = new List<ScaleDefinition>
        {
            new ScaleDefinition("Major", new[] { 0, 2, 4, 5, 7, 9, 11 }, Popularity: 1.2),
            new ScaleDefinition("Minor", new[] { 0, 2, 3, 5, 7, 8, 10 }, Popularity: 1.2),
            new ScaleDefinition("Mixolydian", new[] { 0, 2, 4, 5, 7, 9, 10 }, Popularity: 0.9)
        };
    }

    [Fact]
    public void Match_ShouldReturnEmpty_WhenUserNotesIsEmpty()
    {
        // Arrange
        var userNotes = new List<IReadOnlyCollection<int>>();

        // Act
        var results = _sut.Match(userNotes, _mockLibrary);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void Match_ShouldFindPerfectMatch_RegardlessOfOctave()
    {
        // Arrange
        var userNotes = new List<IReadOnlyCollection<int>> 
        { 
            new[] { 0, 16, 31 } 
        };

        // Act
        var results = _sut.Match(userNotes, _mockLibrary).ToList();

        // Assert
        var topMatch = results.First();
        Assert.Equal("Major", topMatch.Scale.Name);
        Assert.Equal(0, topMatch.RootNote);
        Assert.Empty(topMatch.WrongNotes);
    }

    [Fact]
    public void Match_ShouldIdentifyWrongNotes_WhenScaleDoesNotFullyCoverInput()
    {
        // Arrange
        var userNotes = new List<IReadOnlyCollection<int>> 
        { 
            new[] { 0, 4, 7, 6 } 
        };

        // Act
        var results = _sut.Match(userNotes, _mockLibrary).ToList();

        // Assert
        var cMajorMatch = results.First(r => r.Scale.Name == "Major" && r.RootNote == 0);
        Assert.Contains(6, cMajorMatch.WrongNotes);
    }

    [Fact]
    public void Match_ShouldIdentifyMissingNotes_FromScaleDefinition()
    {
        // Arrange
        var userNotes = new List<IReadOnlyCollection<int>> 
        { 
            new[] { 0, 7 } 
        };

        // Act
        var results = _sut.Match(userNotes, _mockLibrary).ToList();

        // Assert
        var cMajorMatch = results.First(r => r.Scale.Name == "Major" && r.RootNote == 0);
        Assert.Contains(2, cMajorMatch.MissingNotes);
        Assert.Contains(4, cMajorMatch.MissingNotes);
    }

    [Fact]
    public void Match_ShouldDetectTransposedScales()
    {
        // Arrange
        var userNotes = new List<IReadOnlyCollection<int>> 
        { 
            new[] { 2, 6, 9 } 
        };

        // Act
        var results = _sut.Match(userNotes, _mockLibrary).ToList();

        // Assert
        var topMatch = results.First();
        Assert.Equal("Major", topMatch.Scale.Name);
        Assert.Equal(2, topMatch.RootNote); 
    }

    [Fact]
    public void Match_ShouldOrderResultsByScoreDescending()
    {
        // Arrange
        var userNotes = new List<IReadOnlyCollection<int>> 
        { 
            new[] { 0, 3, 7 } 
        };

        // Act
        var results = _sut.Match(userNotes, _mockLibrary).ToList();

        // Assert
        Assert.True(results[0].Score >= results[1].Score);
    }

    [Fact]
    public void Match_ShouldFavorMajorOverMixolydian_InMainstreamProgression()
    {
        // Arrange
        var progression = new List<IReadOnlyCollection<int>>
        {
            new[] { 5, 9, 0 },   
            new[] { 0, 4, 7 },   
            new[] { 7, 11, 2 },  
            new[] { 0, 4, 7 },   
            new[] { 0, 4, 7, 10} 
        };

        // Act
        var results = _sut.Match(progression, _mockLibrary).ToList();

        // Assert
        var majorResult = results.First(r => r.Scale.Name == "Major" && r.RootNote == 0);
        var mixolydianResult = results.First(r => r.Scale.Name == "Mixolydian" && r.RootNote == 0);
        
        Assert.True(majorResult.Score > mixolydianResult.Score);
        
        var topMatch = results.First();
        Assert.Equal("Major", topMatch.Scale.Name);
        Assert.Equal(0, topMatch.RootNote);
    }

    [Fact]
    public void Match_ShouldCorrectlyIdentify_MinorProgression()
    {
        // Arrange
        var progression = new List<IReadOnlyCollection<int>>
        {
            new[] { 9, 0, 4 },   
            new[] { 2, 5, 9 },   
            new[] { 4, 7, 11 },  
            new[] { 9, 0, 4 }    
        };

        // Act
        var results = _sut.Match(progression, _mockLibrary).ToList();

        // Assert
        var topMatch = results.First();
        Assert.Equal("Minor", topMatch.Scale.Name);
        Assert.Equal(9, topMatch.RootNote);
    }
}
