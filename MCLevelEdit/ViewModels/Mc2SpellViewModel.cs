using ReactiveUI;
using System;

namespace MCLevelEdit.ViewModels;

/// <summary>
/// One Magic Carpet 2 spell for a wizard: whether they start with it, its level, and whether it is blocked.
/// The raw bytes are kept, so values other than 0/1 found in some original levels are only changed when edited.
/// </summary>
public class Mc2SpellViewModel : ReactiveObject
{
    private byte _starting;
    private byte _level;
    private byte _blocked;

    public int Index { get; }
    public string Name { get; }

    public event EventHandler? Changed;

    public Mc2SpellViewModel(int index, string name, byte starting, byte level, byte blocked)
    {
        Index = index;
        Name = name;
        _starting = starting;
        _level = level;
        _blocked = blocked;
    }

    public byte StartingRaw => _starting;
    public byte LevelRaw => _level;
    public byte BlockedRaw => _blocked;

    public bool StartsWith
    {
        get => _starting != 0;
        set => Set(ref _starting, (byte)(value ? 1 : 0), nameof(StartsWith));
    }

    /// <summary>The highest spell level: each MC2 spell has three levels, 0 to 2 (remc2 subspell[3]).</summary>
    public const byte MAX_LEVEL = 2;

    /// <summary>
    /// Spell level, 0 to 2. The game treats anything above 2 as 2 (remc2 Spells.cpp). A few original levels
    /// hold 3, 4 or 255; those are kept until edited, and edits are limited to 0 to 2.
    /// </summary>
    public byte Level
    {
        get => _level;
        set => Set(ref _level, Math.Min(value, MAX_LEVEL), nameof(Level));
    }

    public bool Blocked
    {
        get => _blocked != 0;
        set => Set(ref _blocked, (byte)(value ? 1 : 0), nameof(Blocked));
    }

    private void Set(ref byte field, byte value, string propertyName)
    {
        if (field == value)
        {
            // Still notify: a typed value that was limited (e.g. 5 -> 2) must be redrawn as the limited value
            this.RaisePropertyChanged(propertyName);
            return;
        }
        field = value;
        this.RaisePropertyChanged(propertyName);
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
