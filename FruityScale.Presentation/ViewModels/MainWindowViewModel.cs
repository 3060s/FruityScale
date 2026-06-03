using CommunityToolkit.Mvvm.ComponentModel;
using FruityScale.Application.Contracts;
using FruityScale.Application.Services;
using Microsoft.Extensions.Logging;

namespace FruityScale.Presentation.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;
    private readonly IDialogService _dialogService;
    private readonly IEnvironmentService _environmentService;
    private readonly ISetupService _setupService;
    private readonly ScaleMatchingOrchestrator _orchestrator;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<MainWindowViewModel> _logger;
    
    [ObservableProperty]
    private ViewModelBase? _currentContent;
    
    public MainWindowViewModel(
        ISettingsService settingsService, 
        IDialogService dialogService, 
        IEnvironmentService environmentService,
        ISetupService setupService, 
        ScaleMatchingOrchestrator orchestrator,
        ILoggerFactory loggerFactory,
        ILogger<MainWindowViewModel> logger)
    {
        _settingsService = settingsService;
        _dialogService = dialogService;
        _environmentService = environmentService;
        _setupService = setupService;
        _orchestrator = orchestrator;
        _loggerFactory = loggerFactory;
        _logger = _loggerFactory.CreateLogger<MainWindowViewModel>();
        
        _logger.LogDebug("MainWindowViewModel initialized.");
        DetermineInitialView();
    }
    
    public void DetermineInitialView()
    {
        string currentPath = _settingsService.GetFlStudioPath();

        if (string.IsNullOrEmpty(currentPath))
        {
            _logger.LogInformation("No FL Studio path configured. Routing user to SetupView.");
            
            var setupLogger = _loggerFactory.CreateLogger<SetupViewModel>();
            CurrentContent = new SetupViewModel(this, _setupService, _settingsService, _dialogService, _environmentService, setupLogger);
        }
        else
        {
            _logger.LogInformation("FL Studio path found: {Path}. Routing user to MainDashboardView.", currentPath);
            
            var dashboardLogger = _loggerFactory.CreateLogger<MainDashboardViewModel>();
            CurrentContent = new MainDashboardViewModel(_orchestrator, _settingsService, dashboardLogger);
        }
    }
}