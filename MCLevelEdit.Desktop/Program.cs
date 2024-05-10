using Avalonia;
using Avalonia.Logging;
using Avalonia.ReactiveUI;
using MCLevelEdit.Model.Domain;
using Serilog;
using Splat;
using Splat.Serilog;
using System;
using System.IO;

namespace MCLevelEdit.Desktop;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {

#if DEBUG
            LogLevel logLevel = LogLevel.Debug;
#else
            LogLevel logLevel = LogLevel.Warn;

            if (args.Length > 1 && (args[0].Equals("-d", StringComparison.InvariantCultureIgnoreCase) || args[0].Equals("-debug", StringComparison.InvariantCultureIgnoreCase)))
                logLevel = LogLevel.Debug;
#endif
            // prepare and run your App here
            BuildAvaloniaApp(logLevel).StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            Log.Fatal(e, $"Unhandled Exception: {e.Message}");
        }
        finally
        {
            // This block is optional. 
            // Use the finally-block if you need to clean things up or similar
            Log.Information("Programing Exiting");
            Log.CloseAndFlush();
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp(LogLevel logLevel)
    {
        string path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Globals.APP_DIRECTORY),"log.txt");
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is((Serilog.Events.LogEventLevel)logLevel)
            .WriteTo.File(path, rollOnFileSizeLimit: true)
            .CreateLogger();

        // Then in your service locator initialisation
        Locator.CurrentMutable.UseSerilogFullLogger();

        Log.Information("Logging initialized");

        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace((LogEventLevel)logLevel)
            .UseReactiveUI();
    }
}
