using FruityScale.Application.Contracts;
using Microsoft.Extensions.Logging;

namespace FruityScale.Infrastructure.Services;

public class FlStudioSetupService : ISetupService
{
    private readonly ILogger<FlStudioSetupService> _logger;

    public FlStudioSetupService(ILogger<FlStudioSetupService> logger)
    {
        _logger = logger;
    }
    
    public bool ValidateAndSetup(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            _logger.LogWarning("Validation failed: Provided path is empty.");
            return false;
        }

        try
        {
            string? targetScriptsFolder = ResolveScriptsFolder(path);

            if (targetScriptsFolder == null)
            {
                _logger.LogWarning("Validation failed: Could not resolve FL Studio scripts folder from path: {Path}", path);
                return false;
            }

            Directory.CreateDirectory(targetScriptsFolder);
            
            // path we get the script from
            string sourceScriptPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Resources",
                "FlStudioNotesExporter.pyscript"
            );

            // path we copy the script to
            string targetScriptPath = Path.Combine(
                targetScriptsFolder,
                "FlStudioNotesExporter.pyscript"
            );

            if (!File.Exists(sourceScriptPath))
            {
                _logger.LogError("Setup failed: Source script file not found at: {SourcePath}", sourceScriptPath);
                return false;
            }

            File.Copy(sourceScriptPath, targetScriptPath, overwrite: true);
            
            _logger.LogInformation("Successfully deployed FL Studio script to: {TargetPath}", targetScriptPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred during FL Studio validation and setup.");
            return false;
        }
    }
    
    public string GetNotesJsonPath(string path)
    {
        string? targetScriptsFolder = ResolveScriptsFolder(path);

        if (targetScriptsFolder == null)
        {
            _logger.LogError("Failed to get notes JSON path. Invalid FL Studio path: {Path}", path);
            throw new ArgumentException("Invalid FL Studio path.");
        }

        return Path.Combine(targetScriptsFolder, "notes.json");
    }
    
    private string? ResolveScriptsFolder(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;
        
        string normalizedPath = path
            .Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar)
            .TrimEnd(Path.DirectorySeparatorChar);

        if (normalizedPath.EndsWith("Piano roll scripts", StringComparison.OrdinalIgnoreCase))
        {
            return normalizedPath;
        }
        
        var directoryInfo = new DirectoryInfo(normalizedPath);
        string folderName = directoryInfo.Name;

        // Selected FL Studio folder
        if (folderName.Equals("FL Studio", StringComparison.OrdinalIgnoreCase))
        {
            // Check if provided path contains folders "Image-Line" or "Documents"
            // If that's true then provided path is inside /Documents and not /Program Files
            if (normalizedPath.Contains("Image-Line", StringComparison.OrdinalIgnoreCase) || 
                normalizedPath.Contains("Documents", StringComparison.OrdinalIgnoreCase))
            {
                return Path.Combine(normalizedPath, "Settings", "Piano roll scripts");
            }
            
            // If we were given /Program Files path
            // TODO: we can't use /Program Files path because it need admin privileges so it have to be changed
            string systemFallback = Path.Combine(normalizedPath, "System", "Config", "Piano roll scripts");
            if (Directory.Exists(systemFallback)) return systemFallback;
            
            return Path.Combine(normalizedPath, "Settings", "Piano roll scripts");
        }

        // Windows/Linux: Selected path ends with version like: FL Studio 24
        if (folderName.StartsWith("FL Studio", StringComparison.OrdinalIgnoreCase))
        {
            return Path.Combine(normalizedPath, "System", "Config", "Piano roll scripts");
        }
        
        // macOS: Selected path ends with .app like: FL Studio 2024.app
        if (folderName.EndsWith(".app", StringComparison.OrdinalIgnoreCase) && 
            folderName.Contains("FL Studio", StringComparison.OrdinalIgnoreCase))
        {
            return Path.Combine(normalizedPath, "Contents", "Resources", "FL", "System", "Config", "Piano roll scripts");
        }

        // We check if these paths exist, and we use them as fallback
        string possibleUserPath = Path.Combine(normalizedPath, "Settings", "Piano roll scripts");
        if (Directory.Exists(possibleUserPath)) return possibleUserPath;

        string possibleSystemPath = Path.Combine(normalizedPath, "System", "Config", "Piano roll scripts");
        if (Directory.Exists(possibleSystemPath)) return possibleSystemPath;

        return null;
    }
}