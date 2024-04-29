using System;

using Avalonia;
using Avalonia.Logging;
using Avalonia.ReactiveUI;

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
            LogEventLevel logLevel = LogEventLevel.Debug;
#else
            LogEventLevel logLevel = LogEventLevel.Warning;

            if (args.Length > 1 && (args[0].Equals("-d", StringComparison.InvariantCultureIgnoreCase) || args[0].Equals("-debug", StringComparison.InvariantCultureIgnoreCase)))
                logLevel = LogEventLevel.Debug;
#endif


            // prepare and run your App here
            BuildAvaloniaApp(logLevel).StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
        }
        finally
        {
            // This block is optional. 
            // Use the finally-block if you need to clean things up or similar
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp(LogEventLevel logLevel)
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace(logLevel)
            .UseReactiveUI();
    }
}
