using FruityScale.Application.Contracts;
using FruityScale.Domain.Models;
using FruityScale.Domain.Services;
using Microsoft.Extensions.Logging;

namespace FruityScale.Application.Services;

public class ScaleMatchingOrchestrator
{
    private readonly ILogger<ScaleMatchingOrchestrator> _logger;
    private readonly IScaleMatcher _scaleMatcher;
    private readonly IScaleProvider _scaleProvider;
    private readonly INoteProvider _noteProvider;
    private readonly ISettingsService _settingsService;
    private readonly IEnvironmentService _environmentService;
    private readonly ISetupService _setupService;
    
    public ScaleMatchingOrchestrator(
        ILogger<ScaleMatchingOrchestrator> logger,
        IScaleMatcher scaleMatcher, 
        IScaleProvider scaleProvider, 
        INoteProvider noteProvider,
        ISettingsService settingsService,
        IEnvironmentService environmentService,
        ISetupService setupService)
    {
        _logger = logger;
        _scaleMatcher = scaleMatcher;
        _scaleProvider = scaleProvider;
        _noteProvider = noteProvider;
        _settingsService = settingsService;
        _environmentService = environmentService;
        _setupService = setupService;
    }

    public async Task<IEnumerable<ScaleMatchResult>> GetMatchesAsync()
    {
        _logger.LogInformation("Starting scale matching process.");
        
        string flPath = _settingsService.Current.FlStudioPath;
        if (string.IsNullOrEmpty(flPath)) 
        {
            _logger.LogWarning("Matching aborted: FL Studio path is not configured.");
            return Enumerable.Empty<ScaleMatchResult>();
        }
        
        string flStudioFilePath = _setupService.GetNotesJsonPath(flPath);
        if (!File.Exists(flStudioFilePath)) 
        {
            _logger.LogWarning("Matching aborted: User notes file does not exist at {FilePath}", flStudioFilePath);
            return Enumerable.Empty<ScaleMatchResult>();
        }
        
        try
        {
            string scaleLibraryPath = _environmentService.ScaleLibraryPath;
            
            var scalesTask = _scaleProvider.GetScalesAsync(scaleLibraryPath);
            var notesTask = _noteProvider.LoadNotesAsync(flStudioFilePath);
            
            await Task.WhenAll(scalesTask, notesTask);
            
            var allScales = await scalesTask;
            var userNotes = await notesTask;
            
            // TODO: do something with the warning
            if (userNotes == null || !userNotes.Any())
            {
                _logger.LogWarning("Matching aborted: No user notes were found in the exported file.");
                return Enumerable.Empty<ScaleMatchResult>();
            }
            
            var noteNumbers = userNotes
                .Select(n => n.NoteNumber)
                .Distinct()
                .ToList();
            
            var results = _scaleMatcher.Match(noteNumbers, allScales).ToList();
            
            _logger.LogInformation("Matching completed successfully. Found {Count} scale matches for {NoteCount} distinct notes.", results.Count, noteNumbers.Count);
            
            return results.OrderByDescending(r => r.Score);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during the orchestrator matching execution.");
            return Enumerable.Empty<ScaleMatchResult>();
        }
    }
}