using Avalonia.Platform.Storage;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MCLevelEdit.Utils;

/// <summary>
/// Where a file or folder picker should open: the folder of the path already in the field it fills in.
/// </summary>
public static class PickerStartFolder
{
    /// <summary>
    /// The folder a path names, if it exists: for a folder field the folder itself, for a file field the file's
    /// folder (the file itself need not exist yet). Null when there is no path or that folder doesn't exist, so the
    /// picker opens without a start location.
    /// </summary>
    public static string? FindExistingFolder(string? path, bool isFolder = false)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        try
        {
            var full = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path.Trim()));
            if (Directory.Exists(full))
                return full;
            if (isFolder)
                return null;

            var folder = Path.GetDirectoryName(full);
            return !string.IsNullOrEmpty(folder) && Directory.Exists(folder) ? folder : null;
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException || ex is PathTooLongException || ex is System.Security.SecurityException)
        {
            // Not a usable path: let the picker use its own default
            return null;
        }
    }

    /// <summary>The picker start location for a path, or null to open without one (see <see cref="FindExistingFolder"/>).</summary>
    public static async Task<IStorageFolder?> GetAsync(IStorageProvider storageProvider, string? path, bool isFolder = false)
    {
        var folder = FindExistingFolder(path, isFolder);
        return folder is null ? null : await storageProvider.TryGetFolderFromPathAsync(folder);
    }
}
