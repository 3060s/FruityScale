using System.Text.Json;
using FruityScale.Application.Contracts;
using FruityScale.Domain.Models;
using Microsoft.Extensions.Logging;

namespace FruityScale.Infrastructure.Persistence;

public class JsonNoteProvider : INoteProvider
{
    private readonly ILogger<JsonNoteProvider> _logger;

    public JsonNoteProvider(ILogger<JsonNoteProvider> logger)
    {
        _logger = logger;
    }
    
    public async Task<List<NoteEvent>> LoadNotesAsync(string filePath)
    {
        _logger.LogInformation("Attempting to load notes from file: {FilePath}", filePath);
        
        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("notes.json file does not exist at path: {FilePath}", filePath);
                return new List<NoteEvent>();
            }
            
            await using var stream = File.OpenRead(filePath);
            
            // Ignore case
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var notes = await JsonSerializer.DeserializeAsync<List<NoteEvent>>(stream, options) ?? new();
            
            _logger.LogInformation("Successfully deserialized {Count} notes from JSON.", notes.Count);
            return notes;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse notes.json. The file structure might be corrupted.");
            return new List<NoteEvent>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while reading notes file at {FilePath}", filePath);
            return new List<NoteEvent>();
        }
    }
}