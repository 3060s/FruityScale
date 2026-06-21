using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FruityScale.Application.Contracts;
using FruityScale.Application.Services;
using FruityScale.Domain.Models;
using Microsoft.Extensions.Logging;

namespace FruityScale.Presentation.ViewModels;

public partial class MainDashboardViewModel : ViewModelBase
{
    private readonly ScaleMatchingOrchestrator _orchestrator;
    private readonly ISettingsService _settingsService;
    private readonly ISetupService _setupService;
    private readonly ILogger<MainDashboardViewModel> _logger;

    [ObservableProperty]
    private string _dawPath;

    [ObservableProperty]
    private bool _isScanning;
    
    [ObservableProperty]
    private bool _hasNoResults = true;
    
    [ObservableProperty]
    private bool _isGifExpanded;

    [ObservableProperty]
    private string _statusMessage = "Ready to scan. Run FLNotesExport script in FL studio, and click Scan.";
    
    public ObservableCollection<ScaleMatchItemViewModel> ScanResults { get; } = new();

    public MainDashboardViewModel(
        ScaleMatchingOrchestrator orchestrator, 
        ISettingsService settingsService,
        ISetupService setupService,
        ILogger<MainDashboardViewModel> logger)
    {
        _orchestrator = orchestrator;
        _settingsService = settingsService;
        _setupService = setupService;
        _logger = logger;
        
        _dawPath = _settingsService.Current.FlStudioPath;
        _setupService.ValidateAndSetup(_dawPath);
        _logger.LogDebug("MainDashboardViewModel initialized with DAW path: {DawPath}", _dawPath);
    }
    
    [RelayCommand]
    private void ToggleGifSize() => IsGifExpanded = !IsGifExpanded;
    
    [RelayCommand]
    private async Task ScanNotesAsync()
    {
        _logger.LogInformation("User triggered 'Scan Piano Roll' operation.");
        
        IsScanning = true;
        StatusMessage = "Scanning and matching scales...";
        ScanResults.Clear();
        HasNoResults = false;

        try
        {
            var results = await _orchestrator.GetMatchesAsync();
            
            int count = 0;
            foreach (var match in results)
            {
                ScanResults.Add(new ScaleMatchItemViewModel(match));
                count++;
                // TODO: this add this number to some defines in settings later so user can change number of results etc
                if (count >= 40) break;
            }
            
            if (ScanResults.Count == 0)
            {
                _logger.LogWarning("Scan completed, but no match results were loaded into the UI. Target file might be empty or missing.");
                StatusMessage = "notes.json file not found or it's empty. Are you sure you have run script in FL Studio?";
            }
            else
            {
                _logger.LogInformation("Successfully populated UI with {Count} scale match results.", ScanResults.Count);
                StatusMessage = $"Success! Found {ScanResults.Count} matches.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred during UI note scanning update.");
            StatusMessage = $"Scanning error: {ex.Message}";
        }
        finally
        {
            IsScanning = false;
            HasNoResults = ScanResults.Count == 0;
        }
    }
}