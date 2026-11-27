namespace MCLevelEdit.Model.Abstractions;

/// <summary>What a recovery file holds: where it is, and the level it is a copy of.</summary>
public class RecoveryInfo
{
    /// <summary>The recovery file itself (a level file).</summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>The file the level was opened from or last saved to (empty for a level never saved).</summary>
    public string? OriginalFilePath { get; set; }

    /// <summary>The level was marked Hidden Worlds (not kept in the level file itself).</summary>
    public bool HiddenWorlds { get; set; }

    public DateTime SavedAt { get; set; }

    /// <summary>Why it was saved: autosave, or the error the editor met.</summary>
    public string? Reason { get; set; }
}

/// <summary>
/// The recovery file: a copy of the open level saved now and then, and when the editor meets an error, so the work is
/// not lost if it closes without the level being saved. It is cleared when the editor closes normally.
/// </summary>
public interface IRecoveryService
{
    /// <summary>Saves the open level to the recovery file. Never throws: false if it could not.</summary>
    bool SaveRecovery(string reason);

    /// <summary>The recovery file left from before (the editor did not close normally), or null.</summary>
    RecoveryInfo? FindRecovery();

    /// <summary>
    /// The recovery file found at start has been dealt with (opened, or set aside): from now on the open level is
    /// saved over it. Until then it is not, so it cannot be lost before the person decides.
    /// </summary>
    void RecoveryHandled();

    /// <summary>The recovery file set aside (renamed, not deleted) when the person chose not to open it.</summary>
    void DiscardRecovery();

    /// <summary>The recovery file deleted: the editor closed normally.</summary>
    void ClearRecovery();
}
