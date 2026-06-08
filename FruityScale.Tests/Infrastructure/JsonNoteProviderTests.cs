using System.Text.Json;
using FruityScale.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace FruityScale.Tests.Infrastructure;

public class JsonNoteProviderTests : IDisposable
{
    private readonly string _tempAppFolder;
    private readonly string _tempFilePath;
    private readonly ILogger<JsonNoteProvider> _loggerMock;

    public JsonNoteProviderTests()
    {
        // ARRANGE - Common setup for all tests
        _tempAppFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _tempFilePath = Path.Combine(_tempAppFolder, "notes.json");

        _loggerMock = Substitute.For<ILogger<JsonNoteProvider>>();
    }

    [Fact]
    public async Task LoadNotesAsync_WhenFileDoesNotExist_ReturnsEmptyListAndLogsWarning()
    {
        // ARRANGE
        var provider = new JsonNoteProvider(_loggerMock);
        // Ensure file does not exist by not creating the directory or file

        // ACT
        var result = await provider.LoadNotesAsync(_tempFilePath);

        // ASSERT
        Assert.NotNull(result);
        Assert.Empty(result);

        // Verify warning was logged
        _loggerMock.Received().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("notes.json file does not exist")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task LoadNotesAsync_WhenFileHasValidJson_ReturnsDeserializedNotes()
    {
        // ARRANGE
        Directory.CreateDirectory(_tempAppFolder);
        var provider = new JsonNoteProvider(_loggerMock);

        // Create a dummy JSON array for NoteEvent objects
        var validJson = @"
        [
            { ""pitch"": 60, ""velocity"": 100 },
            { ""pitch"": 64, ""velocity"": 90 }
        ]";
        await File.WriteAllTextAsync(_tempFilePath, validJson);

        // ACT
        var result = await provider.LoadNotesAsync(_tempFilePath);

        // ASSERT
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        
        // Verify success log
        _loggerMock.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Successfully deserialized 2 notes")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task LoadNotesAsync_WhenFileContainsInvalidJson_ReturnsEmptyListAndLogsError()
    {
        // ARRANGE
        Directory.CreateDirectory(_tempAppFolder);
        var provider = new JsonNoteProvider(_loggerMock);

        // Malformed JSON string
        await File.WriteAllTextAsync(_tempFilePath, "[ { corrupted_json: ");

        // ACT
        var result = await provider.LoadNotesAsync(_tempFilePath);

        // ASSERT
        Assert.NotNull(result);
        Assert.Empty(result);

        // Verify JsonException was caught and logged
        _loggerMock.Received().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Failed to parse notes.json")),
            Arg.Any<JsonException>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task LoadNotesAsync_WhenUnexpectedExceptionOccurs_ReturnsEmptyListAndLogsError()
    {
        // ARRANGE
        Directory.CreateDirectory(_tempAppFolder);
        var provider = new JsonNoteProvider(_loggerMock);
        
        // Create an empty file first
        await File.WriteAllTextAsync(_tempFilePath, "[]");

        // Lock the file to force an IOException when the provider tries to open it
        using var fileLock = new FileStream(_tempFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        // ACT
        var result = await provider.LoadNotesAsync(_tempFilePath);

        // ASSERT
        Assert.NotNull(result);
        Assert.Empty(result);

        // Verify generic Exception was caught and logged
        _loggerMock.Received().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Unexpected error while reading notes file")),
            Arg.Any<IOException>(), // The underlying exception should be an IOException
            Arg.Any<Func<object, Exception?, string>>());
    }

    public void Dispose()
    {
        // CLEANUP - Remove temporary files and directories after each test runs
        if (Directory.Exists(_tempAppFolder))
        {
            try
            {
                Directory.Delete(_tempAppFolder, recursive: true);
            }
            catch
            {
                // Ignore cleanup errors during test teardown
            }
        }
    }
}