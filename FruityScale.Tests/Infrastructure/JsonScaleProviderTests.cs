using System.Text;
using System.Text.Json;
using FruityScale.Application.Contracts;
using FruityScale.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FruityScale.Tests.Infrastructure;

public class JsonScaleProviderTests
{
    private readonly IEnvironmentService _environmentServiceMock;
    private readonly ILogger<JsonScaleProvider> _loggerMock;

    public JsonScaleProviderTests()
    {
        // ARRANGE - Common setup for all tests
        _environmentServiceMock = Substitute.For<IEnvironmentService>();
        _loggerMock = Substitute.For<ILogger<JsonScaleProvider>>();
    }

    [Fact]
    public async Task GetScalesAsync_WhenFileDoesNotExist_ReturnsEmptyListAndLogsWarning()
    {
        // ARRANGE
        _environmentServiceMock.GetScaleLibraryStream().Returns((Stream)null!);
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
            Arg.Is<object>(o => o.ToString()!.Contains("Scales library stream is null")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task GetScalesAsync_WhenFileHasValidJson_ReturnsDeserializedScales()
    {
        // ARRANGE
        var validJson = @"
        [
            { ""name"": ""Major Scale"" },
            { ""name"": ""Minor Scale"" }
        ]";
        
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(validJson));
        _environmentServiceMock.GetScaleLibraryStream().Returns(stream);
        
        var provider = new JsonScaleProvider(_loggerMock, _environmentServiceMock);

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
        var invalidJson = "[ { invalid_json } ]";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(invalidJson));
        _environmentServiceMock.GetScaleLibraryStream().Returns(stream);

        var provider = new JsonScaleProvider(_loggerMock, _environmentServiceMock);

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
        _environmentServiceMock.GetScaleLibraryStream().Throws(new IOException("Simulated IO device failure"));

        var provider = new JsonScaleProvider(_loggerMock, _environmentServiceMock);

        // ACT
        var result = await provider.GetScalesAsync();

        // ASSERT
        Assert.NotNull(result);
        Assert.Empty(result);

        // Verify generic Exception was caught and logged
        _loggerMock.Received().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Unexpected error while reading scale library stream")),
            Arg.Any<IOException>(), 
            Arg.Any<Func<object, Exception?, string>>());
    }
}