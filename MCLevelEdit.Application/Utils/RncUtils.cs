namespace MCLevelEdit.Application.Utils;

public static class RncUtils
{
    /// <summary>"RNC" + method, unpacked size, packed size, two CRCs, leeway and chunk count.</summary>
    public const int RNC_HEADER_SIZE = 18;

    /// <summary>
    /// Where the last RNC block in the data ends. Packed level files such as Magic Carpet 2's LEVELS.DAT
    /// can be followed by megabytes of padding, which RncProPack's search would otherwise step through
    /// one byte at a time. Returns the data length when no RNC block is found.
    /// </summary>
    public static uint FindEndOfRncData(byte[] data)
    {
        long end = 0;
        for (int i = 0; i + RNC_HEADER_SIZE <= data.Length; i++)
        {
            if (data[i] != 'R' || data[i + 1] != 'N' || data[i + 2] != 'C' || (data[i + 3] != 1 && data[i + 3] != 2))
                continue;

            uint packedSize = (uint)(data[i + 8] << 24 | data[i + 9] << 16 | data[i + 10] << 8 | data[i + 11]);
            long blockEnd = (long)i + RNC_HEADER_SIZE + packedSize;

            // A packed size running past the data means this is not a real header
            if (blockEnd <= data.Length && blockEnd > end)
                end = blockEnd;
        }
        return end > 0 ? (uint)end : (uint)data.Length;
    }

    // RncProPack names each unpacked block after its address in hex, e.g. data_00073F.bin
    private static readonly System.Text.RegularExpressions.Regex ExtractedName =
        new(@"^data_([0-9A-Fa-f]{6})\.bin$", System.Text.RegularExpressions.RegexOptions.Compiled);

    /// <summary>
    /// Prefixes RncProPack's output (data_XXXXXX.bin) with a number in address order, e.g. 002_data_00073F.bin,
    /// so files list in level order. Hex addresses alone sort out of order in Windows Explorer, which compares
    /// digit runs as numbers (data_00A123 comes before data_001000). In Magic Carpet's LEVELS.DAT the levels are
    /// stored in game order, so the number is the level's position. Files already numbered are left alone.
    /// </summary>
    /// <returns>The number of files renamed.</returns>
    public static int NumberExtractedFiles(string folder)
    {
        var files = Directory.GetFiles(folder)
            .Select(path => (path, match: ExtractedName.Match(Path.GetFileName(path))))
            .Where(f => f.match.Success)
            .OrderBy(f => Convert.ToUInt32(f.match.Groups[1].Value, 16))
            .ToArray();

        int number = 0;
        foreach (var (path, _) in files)
        {
            number++;
            var target = Path.Combine(folder, $"{number:D3}_{Path.GetFileName(path)}");
            File.Move(path, target, overwrite: true);
        }
        return files.Length;
    }
}
