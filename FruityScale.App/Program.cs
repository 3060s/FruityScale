using Avalonia;
using System;
using System.IO;
using System.Threading.Tasks;
using Serilog;
using Serilog.Enrichers.ShortTypeName;

namespace FruityScale;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // TODO: save this somewhere because it's also used by JsonSettingsService
        var appFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".fruityscale");
        var logPath = Path.Combine(appFolder, "logs", "fruityscale-.txt");
        
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.WithShortTypeName()
            .WriteTo.Console(
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{ShortTypeName}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                logPath,
                rollingInterval: RollingInterval.Day, // new file every 24h
                retainedFileCountLimit: 10, // max 10 .log files
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{ShortTypeName}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
        
        try
        {
            Log.Information("Application starting up...");
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            
            Log.Information("Application shutdown by user.");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly!");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}