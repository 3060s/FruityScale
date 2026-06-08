using System.Text.Json;
using FruityScale.Application.Contracts;
using FruityScale.Domain.Models;
using FruityScale.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace FruityScale.Tests.Infrastructure;

public class JsonSettingsServiceTests : IDisposable
{
    private readonly string _tempAppFolder;
    private readonly string _tempConfigFilePath;
    private readonly IEnvironmentService _environmentServiceMock;
    private readonly ILogger<JsonSettingsService> _loggerMock;

    public JsonSettingsServiceTests()
    {
        // ARRANGE - Common setup for all tests
        _tempAppFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _tempConfigFilePath = Path.Combine(_tempAppFolder, "settings.json");

        _environmentServiceMock = Substitute.For<IEnvironmentService>();
        _environmentServiceMock.AppFolder.Returns(_tempAppFolder);
        _environmentServiceMock.ConfigFilePath.Returns(_tempConfigFilePath);

        _loggerMock = Substitute.For<ILogger<JsonSettingsService>>();
    }

    [Fact]
    public void Constructor_WhenConfigFileDoesNotExist_CreatesDefaultSettingsAndSavesToFile()
    {
        // ARRANGE
        // The file does not exist initially (guaranteed by using a new Guid for the folder)

        // ACT
        var service = new JsonSettingsService(_loggerMock, _environmentServiceMock);

        // ASSERT
        Assert.NotNull(service.Current);
        Assert.True(File.Exists(_tempConfigFilePath));

        var savedJson = File.ReadAllText(_tempConfigFilePath);
        Assert.False(string.IsNullOrWhiteSpace(savedJson));
    }

    [Fact]
    public void Constructor_WhenConfigFileExistsWithValidJson_LoadsSettingsSuccessfully()
    {
        // ARRANGE
        Directory.CreateDirectory(_tempAppFolder);
        var expectedSettings = new UserSettings(); 
        var json = JsonSerializer.Serialize(expectedSettings);
        File.WriteAllText(_tempConfigFilePath, json);

        // ACT
        var service = new JsonSettingsService(_loggerMock, _environmentServiceMock);

        // ASSERT
        Assert.NotNull(service.Current);
        // If UserSettings has properties, you would assert them here to ensure proper deserialization
        // e.g., Assert.Equal(expectedSettings.SomeProperty, service.Current.SomeProperty);
    }

    [Fact]
    public void Constructor_WhenConfigFileContainsInvalidJson_FallsBackToDefaultsAndLogsError()
    {
        // ARRANGE
        Directory.CreateDirectory(_tempAppFolder);
        File.WriteAllText(_tempConfigFilePath, "{ invalid_json: ");

        // ACT
        var service = new JsonSettingsService(_loggerMock, _environmentServiceMock);

        // ASSERT
        Assert.NotNull(service.Current); // Should fallback to new UserSettings()
        
        // Verify that LogError was called on the logger
        _loggerMock.Received().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Error reading config file")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public void Update_WhenCalled_ModifiesCachedSettingsAndSavesToDisk()
    {
        // ARRANGE
        var service = new JsonSettingsService(_loggerMock, _environmentServiceMock);
        var initialJson = File.ReadAllText(_tempConfigFilePath);

        // ACT
        service.Update(settings =>
        {
            // Note: Replace this with an actual property from your UserSettings class
            // settings.Theme = "Dark"; 
        });

        // ASSERT
        var updatedJson = File.ReadAllText(_tempConfigFilePath);
        
        // Ensure the file was actually written to. 
        // If you modify a property in the ACT section, assert that the new JSON contains that change.
        Assert.NotNull(updatedJson);
        Assert.True(File.Exists(_tempConfigFilePath));
    }

    [Fact]
    public void Update_WhenFileCannotBeWritten_LogsErrorAndDoesNotThrow()
    {
        // ARRANGE
        var service = new JsonSettingsService(_loggerMock, _environmentServiceMock);

        // Open the file with exclusive access to simulate a locked file (e.g., access denied or in use)
        using var fileStream = new FileStream(_tempConfigFilePath, FileMode.Open, FileAccess.Read, FileShare.None);

        // ACT
        var exception = Record.Exception(() => service.Update(settings => { }));

        // ASSERT
        Assert.Null(exception); // The service should swallow the exception and log it, not throw
        
        _loggerMock.Received().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Failed to save config")),
            Arg.Any<Exception>(),
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
                // Ignore cleanup errors in tests (e.g., file locked by OS delayed release)
            }
        }
    }
}