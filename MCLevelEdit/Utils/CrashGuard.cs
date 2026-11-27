using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using MCLevelEdit.Model.Abstractions;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using Serilog;
using Splat;
using System;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;

namespace MCLevelEdit.Utils;

/// <summary>
/// Keeps the editor from closing on an error and losing the level: an error in a command or on the UI thread is logged,
/// the level saved to the recovery file and the person told, and the editor carries on. An error it cannot carry on from
/// (on another thread) still saves the recovery file before the editor closes. The level is also saved to the recovery
/// file every few minutes, for when the editor is closed in a way no code sees (the power going off).
/// </summary>
public static class CrashGuard
{
    private static readonly TimeSpan AutosaveInterval = TimeSpan.FromMinutes(2);

    // recovery saves after errors no closer together than this (an error that repeats, say in drawing, every frame)
    private static readonly TimeSpan ErrorSaveInterval = TimeSpan.FromSeconds(5);

    private static DateTime _lastErrorSave = DateTime.MinValue;
    private static int _messageShowing;
    private static DispatcherTimer? _autosaveTimer;

    private static IRecoveryService? Recovery => Locator.Current.GetService<IRecoveryService>();

    /// <summary>Set up once the app's services are registered.</summary>
    public static void Install(IClassicDesktopStyleApplicationLifetime? desktop)
    {
        // another thread: the editor closes whatever is done, but the level is saved first
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            var ex = e.ExceptionObject as Exception;
            Log.Fatal(ex, $"Unhandled exception, the editor is closing: {ex?.Message}");
            SaveRecovery($"Fatal error: {ex?.Message}");
            Log.CloseAndFlush();
        };

        // a task nobody waited on failed: logged only (it does not close the editor)
        TaskScheduler.UnobservedTaskException += (sender, e) =>
        {
            Log.Error(e.Exception, $"Unobserved task exception: {e.Exception.Message}");
            e.SetObserved();
        };

        // a command failed (ReactiveUI would throw it again on the UI thread, closing the editor)
        RxApp.DefaultExceptionHandler = Observer.Create<Exception>(OnError);

        // anything else thrown on the UI thread
        Dispatcher.UIThread.UnhandledException += (sender, e) =>
        {
            e.Handled = true;
            OnError(e.Exception);
        };

        _autosaveTimer = new DispatcherTimer { Interval = AutosaveInterval };
        _autosaveTimer.Tick += (sender, e) => SaveRecovery("Autosave");
        _autosaveTimer.Start();

        // closed normally (the person was asked to save): the recovery file is no longer needed. Any other way out keeps it.
        if (desktop is not null)
            desktop.Exit += (sender, e) =>
            {
                _autosaveTimer?.Stop();
                if (ClosedNormally)
                    Recovery?.ClearRecovery();
            };
    }

    /// <summary>The person closed the editor and was asked to save their level first: the recovery file can go.</summary>
    public static bool ClosedNormally { get; set; }

    /// <summary>Saves the open level to the recovery file. Never throws.</summary>
    public static bool SaveRecovery(string reason)
    {
        try
        {
            return Recovery?.SaveRecovery(reason) ?? false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>An error the editor carries on from: logged, the level saved to the recovery file, the person told.</summary>
    public static void OnError(Exception ex)
    {
        try
        {
            Log.Error(ex, $"Unhandled exception: {ex.Message}");

            bool saved = false;
            if (DateTime.Now - _lastErrorSave > ErrorSaveInterval)
            {
                _lastErrorSave = DateTime.Now;
                saved = SaveRecovery($"Error: {ex.Message}");
            }

            // one message at a time
            Dispatcher.UIThread.Post(async () =>
            {
                if (Interlocked.Exchange(ref _messageShowing, 1) == 1)
                    return;
                try
                {
                    string recovery = saved
                        ? "A copy of your level has been saved to the recovery file in Documents\\MCLevelEdit\\Recovery. "
                        : string.Empty;
                    var box = MessageBoxManager.GetMessageBoxStandard("Error",
                        $"Something went wrong: {ex.Message}\n\nThe editor has kept running. {recovery}" +
                        "Save your level (Save As, to a new file is safest) and restart the editor if anything looks wrong.\n\n" +
                        "Details are in log.txt in Documents\\MCLevelEdit.",
                        ButtonEnum.Ok, Icon.Error);
                    await box.ShowAsync();
                }
                catch (Exception boxEx)
                {
                    Log.Error(boxEx, "Unable to show the error message");
                }
                finally
                {
                    Interlocked.Exchange(ref _messageShowing, 0);
                }
            });
        }
        catch
        {
            // reporting the error must never itself close the editor
        }
    }
}
