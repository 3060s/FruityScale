using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FruityScale.Application.Contracts;
using Microsoft.Extensions.Logging;

namespace FruityScale.Presentation.ViewModels;

public partial class SetupViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainNavigation;
    private readonly ISetupService _setupService;
    private readonly ISettingsService _settingsService;
    private readonly IDialogService _dialogService;
    private readonly IEnvironmentService _environmentService;
    private readonly ILogger<SetupViewModel> _logger;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmPathCommand))]
    private string _selectedPath = string.Empty;
    
    [ObservableProperty]
    private string _errorMessage = string.Empty;
    
    [ObservableProperty]
    private string _examplePathHint = string.Empty;

    public SetupViewModel(
        MainWindowViewModel mainNavigation,
        ISetupService setupService,
        ISettingsService settingsService,
        IDialogService dialogService,
        IEnvironmentService environmentService,
        ILogger<SetupViewModel> logger)
    {
        _mainNavigation = mainNavigation;
        _setupService = setupService;
        _settingsService = settingsService;
        _dialogService = dialogService;
        _environmentService = environmentService;
        _logger = logger;
        
        ExamplePathHint = _environmentService.DefaultFlStudioPath;
        
        _logger.LogDebug("SetupViewModel initialized.");
    }
    
    [RelayCommand]
    private async Task BrowseFolderAsync()
    {
        _logger.LogDebug("Opening folder picker dialog...");
        
        var path = await _dialogService.SelectFolderAsync("Select FL Studio or Piano Roll Scripts Folder");
        
        if (!string.IsNullOrWhiteSpace(path))
        {
            _logger.LogInformation("User selected path via dialog: {Path}", path);
            SelectedPath = path;
        }
    }
    
    [RelayCommand(CanExecute = nameof(CanConfirm))]
    private void ConfirmPath()
    {
        ErrorMessage = string.Empty;
        _logger.LogInformation("User is attempting to confirm FL Studio path: {Path}", SelectedPath);
        
        bool success = _setupService.ValidateAndSetup(SelectedPath);

        if (success)
        {
            _logger.LogInformation("Path validation and script setup successful. Saving path...");
            _settingsService.SaveFlStudioPath(SelectedPath);
            
            // Inform MainWindowViewModel to change view after correct setup
            _mainNavigation.DetermineInitialView();
        }
        else
        {
            _logger.LogWarning("Path validation or script setup failed for path: {Path}", SelectedPath);
            ErrorMessage = "Directory is invalid or script installation failed. Make sure you have selected correct directory.";
        }
    }

    private bool CanConfirm() => !string.IsNullOrWhiteSpace(SelectedPath);
}