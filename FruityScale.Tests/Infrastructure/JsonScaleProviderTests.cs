using System.Text.Json;
using FruityScale.Application.Contracts;
using FruityScale.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace FruityScale.Tests.Infrastructure;

public class JsonScaleProviderTests : IDisposable
{
    private readonly string _tempAppFolder;
    private readonly string _tempFilePath;
    private readonly IEnvironmentService _environmentServiceMock;
    private readonly ILogger<JsonScaleProvider> _loggerMock;

    public JsonScaleProviderTests()
    {
        // ARRANGE - Common setup for all tests
        _tempAppFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _tempFilePath = Path.Combine(_tempAppFolder, "scales.json");

        _environmentServiceMock = Substitute.For<IEnvironmentService>();
        _environmentServiceMock.ScaleLibraryPath.Returns(_tempFilePath);

        _loggerMock = Substitute.For<ILogger<JsonScaleProvider>>();
    }

    [Fact]
    public async Task GetScalesAsync_WhenFileDoesNotExist_ReturnsEmptyListAndLogsWarning()
    {
        // ARRANGE
        var provider = new JsonScaleProvider(_loggerMock, _environmentServiceMock);

        // ACT
        var result = await provider.GetScalesAsync();

        // ASSERT
        Assert.NotNull(result);
        Assert.Empty(result);

        // Verify warning was logged
        _loggerMock.Received().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Scales library file does not exist")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task GetScalesAsync_WhenFileHasValidJson_ReturnsDeserializedScales()
    {
        // ARRANGE
        Directory.CreateDirectory(_tempAppFolder);
        var provider = new JsonScaleProvider(_loggerMock, _environmentServiceMock);

        // Create a dummy JSON array. 
        // Note: Using lowercase property names to test the PropertyNameCaseInsensitive = true behavior
        var validJson = @"
        [
            { ""name"": ""Major Scale"" },
            { ""name"": ""Minor Scale"" }
        ]";
        await File.WriteAllTextAsync(_tempFilePath, validJson);

        // ACT
        var result = await provider.GetScalesAsync();

        // ASSERT
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        
        _loggerMock.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Successfully loaded 2 scales")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task GetScalesAsync_WhenFileContainsInvalidJson_ReturnsEmptyListAndLogsError()
    {
        // ARRANGE
        Directory.CreateDirectory(_tempAppFolder);
        var provider = new JsonScaleProvider(_loggerMock, _environmentServiceMock);

        // Malformed JSON string
        await File.WriteAllTextAsync(_tempFilePath, "[ { invalid_json } ]");

        // ACT
        var result = await provider.GetScalesAsync();

        // ASSERT
        Assert.NotNull(result);
        Assert.Empty(result);

        // Verify JsonException was caught and logged
        _loggerMock.Received().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Failed to parse scale library JSON")),
            Arg.Any<JsonException>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task GetScalesAsync_WhenUnexpectedExceptionOccurs_ReturnsEmptyListAndLogsError()
    {
        // ARRANGE
        Directory.CreateDirectory(_tempAppFolder);
        var provider = new JsonScaleProvider(_loggerMock, _environmentServiceMock);
        
        // Create an empty file first
        await File.WriteAllTextAsync(_tempFilePath, "[]");

        // Lock the file to force an IOException when the provider tries to open it
        using var fileLock = new FileStream(_tempFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        // ACT
        var result = await provider.GetScalesAsync();

        // ASSERT
        Assert.NotNull(result);
        Assert.Empty(result);

        // Verify generic Exception was caught and logged
        _loggerMock.Received().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Unexpected error while reading scale library file")),
            Arg.Any<IOException>(), // The underlying exception should be an IOException
            Arg.Any<Func<object, Exception?, string>>());
    }

    public void Dispose()
    {
        // CLEANUP - Remove temporary files and directories after each test
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