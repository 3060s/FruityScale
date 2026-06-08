using System.Text.Json;
using FruityScale.Application.Contracts;
using FruityScale.Domain.Models;
using Microsoft.Extensions.Logging;

namespace FruityScale.Infrastructure.Persistence;

public class JsonScaleProvider : IScaleProvider
{
    private readonly ILogger<JsonScaleProvider> _logger;
    private readonly IEnvironmentService _environmentService;

    public JsonScaleProvider(
        ILogger<JsonScaleProvider> logger, 
        IEnvironmentService environmentService)
    {
        _logger = logger;
        _environmentService = environmentService;
    }
    
    public async Task<List<ScaleDefinition>> GetScalesAsync()
    {
        string filePath = _environmentService.ScaleLibraryPath;
        
        _logger.LogInformation("Attempting to load scales library from file: {FilePath}", filePath);
        //if (!File.Exists(filePath)) return new List<ScaleDefinition>();
        
        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Scales library file does not exist at path: {FilePath}", filePath);
                return new List<ScaleDefinition>();
            }

            await using var stream = File.OpenRead(filePath);
            
            // Ignore case
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var scales = await JsonSerializer.DeserializeAsync<List<ScaleDefinition>>(stream, options) ?? new();
            
            _logger.LogInformation("Successfully loaded {Count} scales from library.", scales.Count);
            return scales;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse scale library JSON. Check if the file format matches ScaleDefinition.");
            return new List<ScaleDefinition>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while reading scale library file at {FilePath}", filePath);
            return new List<ScaleDefinition>();
        }
    }
}