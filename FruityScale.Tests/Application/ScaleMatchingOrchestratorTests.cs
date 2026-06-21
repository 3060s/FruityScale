using FruityScale.Application.Contracts;
using FruityScale.Application.Services;
using FruityScale.Domain.Models;
using FruityScale.Domain.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace FruityScale.Tests.Application;

public class ScaleMatchingOrchestratorTests : IDisposable
{
    private readonly ILogger<ScaleMatchingOrchestrator> _logger;
    private readonly IScaleMatcher _scaleMatcher;
    private readonly IScaleProvider _scaleProvider;
    private readonly INoteProvider _noteProvider;
    private readonly ISettingsService _settingsService;
    private readonly ISetupService _setupService;
    private readonly IScorePartitioner _scorePartitioner;
    private readonly ScaleMatchingOrchestrator _sut;
    
    private readonly string _tempTestFile;

    public ScaleMatchingOrchestratorTests()
    {
        _logger = Substitute.For<ILogger<ScaleMatchingOrchestrator>>();
        _scaleMatcher = Substitute.For<IScaleMatcher>();
        _scaleProvider = Substitute.For<IScaleProvider>();
        _noteProvider = Substitute.For<INoteProvider>();
        _settingsService = Substitute.For<ISettingsService>();
        _setupService = Substitute.For<ISetupService>();
        _scorePartitioner = Substitute.For<IScorePartitioner>();
        
        var defaultSettings = new UserSettings { FlStudioPath = @"C:\Program Files\Image-Line\FL Studio" };
        _settingsService.Current.Returns(defaultSettings);

        _sut = new ScaleMatchingOrchestrator(
            _logger,
            _scaleMatcher,
            _scaleProvider,
            _noteProvider,
            _settingsService,
            _setupService,
            _scorePartitioner
        );

        // Create a real temporary file to satisfy File.Exists(path) checks when needed
        _tempTestFile = Path.GetTempFileName();
    }

    public void Dispose()
    {
        if (File.Exists(_tempTestFile))
        {
            File.Delete(_tempTestFile);
        }
    }

    [Fact]
    public async Task GetMatchesAsync_ShouldReturnEmpty_WhenFlStudioPathIsEmpty()
    {
        // Arrange
        _settingsService.Current.Returns(new UserSettings { FlStudioPath = string.Empty });

        // Act
        var result = await _sut.GetMatchesAsync();

        // Assert
        Assert.Empty(result);
        await _scaleProvider.DidNotReceiveWithAnyArgs().GetScalesAsync();
    }

    [Fact]
    public async Task GetMatchesAsync_ShouldReturnEmpty_WhenNotesJsonFileDoesNotExist()
    {
        // Arrange
        _settingsService.Current.Returns(new UserSettings { FlStudioPath = @"C:\Program Files\Image-Line\FL Studio"});
        _setupService.GetNotesJsonPath(Arg.Any<string>()).Returns(@"C:\NonExistentPath\notes.json");

        // Act
        var result = await _sut.GetMatchesAsync();

        // Assert
        Assert.Empty(result);
        await _scaleProvider.DidNotReceiveWithAnyArgs().GetScalesAsync();
    }

    [Fact]
    public async Task GetMatchesAsync_ShouldReturnEmpty_WhenLoadedNotesCollectionIsEmpty()
    {
        // Arrange
        _settingsService.Current.Returns(new UserSettings { FlStudioPath = @"C:\Program Files\Image-Line\FL Studio"});
        _setupService.GetNotesJsonPath(Arg.Any<string>()).Returns(_tempTestFile);
        
        _scaleProvider.GetScalesAsync().Returns(Task.FromResult(new List<ScaleDefinition>()));
        _noteProvider.LoadNotesAsync(_tempTestFile).Returns(Task.FromResult(new List<NoteEvent>()));

        // Act
        var result = await _sut.GetMatchesAsync();

        // Assert
        Assert.Empty(result);
        _scaleMatcher.DidNotReceiveWithAnyArgs().Match(Arg.Any<IReadOnlyList<IReadOnlyCollection<int>>>(), Arg.Any<IEnumerable<ScaleDefinition>>());
    }

    [Fact]
    public async Task GetMatchesAsync_ShouldCorrectlyProcessAndOrderMatches_WhenDataIsValid()
    {
        // Arrange
        _settingsService.Current.Returns(new UserSettings { FlStudioPath = @"C:\Program Files\Image-Line\FL Studio"});
        _setupService.GetNotesJsonPath(Arg.Any<string>()).Returns(_tempTestFile);

        var mockScales = new List<ScaleDefinition> { new ScaleDefinition("Major", new[] { 0, 2, 4 }, Popularity: 1.2) };
        var mockNotes = new List<NoteEvent> 
        { 
            new NoteEvent(60, "C", "C5", 0, 10), 
            new NoteEvent(64, "E", "E5", 0, 10),
            new NoteEvent(60, "C", "C5", 0, 10) 
        };
        
        var mockPartitions = new List<IReadOnlyCollection<int>> 
        { 
            new[] { 60 }, 
            new[] { 64 } 
        };
        
        _scaleProvider.GetScalesAsync().Returns(Task.FromResult(mockScales));
        _noteProvider.LoadNotesAsync(_tempTestFile).Returns(Task.FromResult(mockNotes));
        _scorePartitioner.Partition(mockNotes).Returns(mockPartitions);

        var expectedResults = new List<ScaleMatchResult>
        {
            new ScaleMatchResult(mockScales[0], 0, 0.5, [], []),
            new ScaleMatchResult(mockScales[0], 0, 1.0, [], [])
        };

        _scaleMatcher.Match(mockPartitions, mockScales).Returns(expectedResults);

        // Act
        var result = (await _sut.GetMatchesAsync()).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(1.0, result[0].Score); 
        Assert.Equal(0.5, result[1].Score);
    }
}
