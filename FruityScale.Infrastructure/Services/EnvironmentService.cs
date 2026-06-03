using FruityScale.Application.Contracts;
using FruityScale.Domain.Enums;

namespace FruityScale.Infrastructure.Services;

public class EnvironmentService : IEnvironmentService
{
    public AppPlatform CurrentPlatform
    {
        get
        {
            if (OperatingSystem.IsWindows()) return AppPlatform.Windows;
            if (OperatingSystem.IsMacOS()) return AppPlatform.MacOS;
            if (OperatingSystem.IsLinux()) return AppPlatform.Linux;
            return AppPlatform.Unknown;
        }
    }

    public string DefaultFlStudioPath
    {
        get
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            
            if (!string.IsNullOrEmpty(documentsPath))
            {
                // Documents is recommended because it's not changed in every FL Studio version
                return Path.Combine(documentsPath, "Image-Line", "FL Studio");
            }
            
            string basePath = GetProgramBasePath(CurrentPlatform);

            if (Directory.Exists(basePath))
            {
                try
                {
                    string searchPattern = CurrentPlatform == AppPlatform.MacOS ? "FL Studio*.app" : "FL Studio*";
                    
                    var detectedDirs = Directory.GetDirectories(basePath, searchPattern);

                    if (detectedDirs.Length > 0)
                    {
                        var latestVersionPath = detectedDirs.OrderByDescending(d => d).First();
                        return latestVersionPath;
                    }
                }
                catch
                {
                    // go to fallback under this catch
                }
            }
            
            // Fallback if documents wasn't found
            // Hardcoded year lol, sorry for this awful code
            return CurrentPlatform switch
            {
                AppPlatform.Windows => @"C:\Program Files\Image-Line\FL Studio 2024",
                // Not sure about the path because I don't have any MacOS device
                AppPlatform.MacOS => "/Applications/FL Studio 2024.app",
                // FL Studio doesn't work natively on Linux so we leave something like that
                // because it's possible to run through Wine
                // it might not be correct
                AppPlatform.Linux => Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".wine", "drive_c", "Program Files", "Image-Line", "FL Studio 2024"),
                _ => "Select your FL Studio or 'Piano roll scripts' directory"
            };
        }
    }
    
    private string GetProgramBasePath(AppPlatform platform)
    {
        return platform switch
        {
            AppPlatform.Windows => @"C:\Program Files\Image-Line",
            
            AppPlatform.MacOS => "/Applications",
            
            AppPlatform.Linux => Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".wine", "drive_c", "Program Files", "Image-Line"),
                
            _ => string.Empty
        };
    }
}