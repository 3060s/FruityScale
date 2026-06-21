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
    private readonly ISetupService _setupService;
    private readonly IScorePartitioner _scorePartitioner;
    
    public ScaleMatchingOrchestrator(
        ILogger<ScaleMatchingOrchestrator> logger,
        IScaleMatcher scaleMatcher, 
        IScaleProvider scaleProvider, 
        INoteProvider noteProvider,
        ISettingsService settingsService,
        ISetupService setupService,
        IScorePartitioner scorePartitioner)
    {
        _logger = logger;
        _scaleMatcher = scaleMatcher;
        _scaleProvider = scaleProvider;
        _noteProvider = noteProvider;
        _settingsService = settingsService;
        _setupService = setupService;
        _scorePartitioner = scorePartitioner;
    }

    public async Task<IEnumerable<ScaleMatchResult>> GetMatchesAsync()
    {
        _logger.LogInformation("Starting scale matching process.");
        
        string flPath = _settingsService.Current.FlStudioPath;
        if (string.IsNullOrEmpty(flPath)) 
        {
            _logger.LogWarning("Matching aborted: FL Studio path is not configured.");
            return [];
        }
        
        string flStudioFilePath = _setupService.GetNotesJsonPath(flPath);
        if (!File.Exists(flStudioFilePath)) 
        {
            _logger.LogWarning("Matching aborted: User notes file does not exist at {FilePath}", flStudioFilePath);
            return [];
        }
        
        try
        {
            var scalesTask = _scaleProvider.GetScalesAsync();
            var notesTask = _noteProvider.LoadNotesAsync(flStudioFilePath);
            
            await Task.WhenAll(scalesTask, notesTask);
            
            var allScales = await scalesTask;
            var userNotes = await notesTask;
            
            if (userNotes.Count == 0)
            {
                _logger.LogWarning("Matching aborted: No user notes were found in the exported file.");
                return [];
            }

            var parts = _scorePartitioner.Partition(userNotes);
            var results = _scaleMatcher
                .Match(parts, allScales)
                .OrderByDescending(r => r.Score)
                .ToList();

            var maxScore = results.FirstOrDefault()?.Score ?? 1;
            
            _logger.LogInformation("Matching completed successfully. Found {Count} scale matches for {NoteCount} notes.", results.Count, userNotes.Count);
            
            return results.Select(r => r with { Score = r.Score / maxScore });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during the orchestrator matching execution.");
            return [];
        }
    }
}
