using MCLevelEdit.Infrastructure.Interfaces;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.Model.Repository;
using Splat;
using System.Text.Json;

namespace MCLevelEdit.Application.Services;

/// <summary>
/// The recovery file, in the editor's folder in Documents (Recovery\Recovery.DAT, with Recovery.json saying which
/// level it is a copy of).
/// </summary>
public class RecoveryService : IRecoveryService, IEnableLogger
{
    private readonly IFilePort _filePort;
    private readonly object _lock = new object();

    // a recovery file was there at start and the person has not decided about it yet: it is not saved over
    private bool _awaitingDecision;

    public string RecoveryFolder { get; }
    public string RecoveryFilePath => Path.Combine(RecoveryFolder, "Recovery.DAT");
    private string InfoFilePath => Path.Combine(RecoveryFolder, "Recovery.json");

    public RecoveryService(IFilePort filePort, string? recoveryFolder = null)
    {
        _filePort = filePort;
        RecoveryFolder = recoveryFolder ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Globals.APP_DIRECTORY, "Recovery");
        _awaitingDecision = File.Exists(RecoveryFilePath);
    }

    public bool SaveRecovery(string reason)
    {
        try
        {
            lock (_lock)
            {
                if (_awaitingDecision)
                    return false;

                var map = MapRepository.Map;
                if (map is null)
                    return false;

                Directory.CreateDirectory(RecoveryFolder);
                // the level written whole before the last copy is replaced: a save that fails leaves the last copy
                if (!_filePort.SaveMap(map, RecoveryFilePath))
                    return false;

                var info = new RecoveryInfo
                {
                    FilePath = RecoveryFilePath,
                    OriginalFilePath = map.FilePath,
                    HiddenWorlds = map.IsHiddenWorlds,
                    SavedAt = DateTime.Now,
                    Reason = reason
                };
                File.WriteAllText(InfoFilePath, JsonSerializer.Serialize(info));
                return true;
            }
        }
        catch (Exception ex)
        {
            try { this.Log().Error(ex, $"Unable to save the recovery file: {ex.Message}"); } catch { }
            return false;
        }
    }

    public RecoveryInfo? FindRecovery()
    {
        try
        {
            if (!File.Exists(RecoveryFilePath))
                return null;

            RecoveryInfo? info = null;
            if (File.Exists(InfoFilePath))
            {
                try { info = JsonSerializer.Deserialize<RecoveryInfo>(File.ReadAllText(InfoFilePath)); }
                catch (Exception ex) { this.Log().Warn(ex, "The recovery file's details could not be read"); }
            }

            info ??= new RecoveryInfo { SavedAt = File.GetLastWriteTime(RecoveryFilePath) };
            info.FilePath = RecoveryFilePath;
            return info;
        }
        catch (Exception ex)
        {
            this.Log().Error(ex, $"Unable to look for a recovery file: {ex.Message}");
            return null;
        }
    }

    public void RecoveryHandled()
    {
        lock (_lock)
            _awaitingDecision = false;
    }

    public void DiscardRecovery()
    {
        lock (_lock)
        {
            try
            {
                // kept as the one before, in case: only the newest set aside is kept
                File.Move(RecoveryFilePath, Path.Combine(RecoveryFolder, "Recovery_Discarded.DAT"), true);
                if (File.Exists(InfoFilePath))
                    File.Move(InfoFilePath, Path.Combine(RecoveryFolder, "Recovery_Discarded.json"), true);
            }
            catch (Exception ex)
            {
                this.Log().Error(ex, $"Unable to set the recovery file aside: {ex.Message}");
            }
            _awaitingDecision = false;
        }
    }

    public void ClearRecovery()
    {
        lock (_lock)
        {
            // one found at start and never decided on is kept for the next time
            if (_awaitingDecision)
                return;
            try
            {
                if (File.Exists(RecoveryFilePath))
                    File.Delete(RecoveryFilePath);
                if (File.Exists(InfoFilePath))
                    File.Delete(InfoFilePath);
            }
            catch (Exception ex)
            {
                this.Log().Error(ex, $"Unable to delete the recovery file: {ex.Message}");
            }
        }
    }
}
