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
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {

#if DEBUG
        LogEventLevel logLevel = LogEventLevel.Debug;
#else
        LogEventLevel logLevel = LogEventLevel.Warning;
#endif

        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace(logLevel)
            .UseReactiveUI();
    }
}
