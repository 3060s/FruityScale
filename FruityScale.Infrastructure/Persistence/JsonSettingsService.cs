using System.Text.Json;
using FruityScale.Application.Contracts;
using FruityScale.Domain.Models;
using Microsoft.Extensions.Logging;

namespace FruityScale.Infrastructure.Persistence;

public class JsonSettingsService : ISettingsService
{
    private readonly ILogger<JsonSettingsService> _logger;
    private readonly string _configPath;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly UserSettings _cachedSettings;
    
    public UserSettings Current => _cachedSettings;

    public JsonSettingsService(ILogger<JsonSettingsService> logger)
    {
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        
        var appFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".fruityscale");
        Directory.CreateDirectory(appFolder);
        _configPath = Path.Combine(appFolder, "config.json");
        
        _cachedSettings = LoadSettingsFromFile();
    }
    
    private UserSettings LoadSettingsFromFile()
    {
        if (!File.Exists(_configPath))
        {
            _logger.LogInformation("Config file not found. Creating default settings.");
            var defaultSettings = new UserSettings();
            SaveToFile(defaultSettings);
            return defaultSettings;
        }

        try
        {
            var json = File.ReadAllText(_configPath);
            var settings = JsonSerializer.Deserialize<UserSettings>(json);
            _logger.LogInformation("Configuration loaded successfully.");
            return settings ?? new UserSettings();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading config file. Falling back to defaults.");
            return new UserSettings();
        }
    }

    public void Update(Action<UserSettings> updateAction)
    {
        updateAction(_cachedSettings);
        SaveToFile(_cachedSettings);
    }

    private void SaveToFile(UserSettings settings)
    {
        try
        {
            var json = JsonSerializer.Serialize(settings, _jsonOptions);
            File.WriteAllText(_configPath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save config to {ConfigPath}", _configPath);
        }
    }
}