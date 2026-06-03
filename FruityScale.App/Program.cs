using Avalonia;
using System;
using System.Threading.Tasks;
using Serilog;

namespace FruityScale;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // TODO: i'm not sure how exactly it works yet, but i think it's good idea if there is .log for every app startup
        // TODO: after sudo kill -SIGSEGV <PID> serilog didnt report Log.Fatal to log file for some reason
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug() // log everything higher than Debug
            .WriteTo.Console()    // log to console
            .WriteTo.File("logs/fruityscale-.txt", // logs location (app directory, inside /logs)
                rollingInterval: RollingInterval.Day, // new file every 24h
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
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