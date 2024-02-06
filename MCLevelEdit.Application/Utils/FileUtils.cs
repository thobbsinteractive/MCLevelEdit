namespace MCLevelEdit.Application.Utils;

public class FileUtils
{
    public static void DeleteExistingFiles(string folderPath)
    {
        string levelsdat = GetFilePath(folderPath, "LEVELS.DAT");
        if (levelsdat != null)
        {
            FileInfo fInfo = new FileInfo(levelsdat);
            if (fInfo.IsReadOnly)
            {
                fInfo.IsReadOnly = false;
            }
            File.Delete(levelsdat);
        }
        string levelstab = GetFilePath(folderPath, "LEVELS.TAB");
        if (levelstab != null)
        {
            FileInfo fInfo = new FileInfo(levelstab);
            if (fInfo.IsReadOnly)
            {
                fInfo.IsReadOnly = false;
            }
            File.Delete(levelstab);
        }
    }

    public static void SetFilesToReadonly(string folderPath)
    {
        string levelsdat = GetFilePath(folderPath, "LEVELS.DAT");
        if (levelsdat != null)
        {
            FileInfo fInfo = new FileInfo(levelsdat);
            fInfo.IsReadOnly = true;

        }
        string levelstab = GetFilePath(folderPath, "LEVELS.TAB");
        if (levelstab != null)
        {
            FileInfo fInfo = new FileInfo(levelstab);
            fInfo.IsReadOnly = true;
        }
    }

    public static void CopyBackupFiles(string folderPath)
    {
        string levelsdat = GetFilePath(folderPath, "LEVELS.DAT");
        if (levelsdat != null)
        {
            var backupPath = Path.Combine(Path.GetDirectoryName(levelsdat), "backup");
            if (!Directory.Exists(backupPath))
            {
                Directory.CreateDirectory(backupPath);
            }

            if (Directory.Exists(backupPath))
            {
                File.Copy(levelsdat, Path.Combine(backupPath, "LEVELS.DAT"), false);
            }
        }

        string levelstab = GetFilePath(folderPath, "LEVELS.TAB");
        if (levelstab != null)
        {
            var backupPath = Path.Combine(Path.GetDirectoryName(levelstab), "backup");
            if (!Directory.Exists(backupPath))
            {
                Directory.CreateDirectory(backupPath);
            }

            if (Directory.Exists(backupPath))
            {
                File.Copy(levelstab, Path.Combine(backupPath, "LEVELS.TAB"), false);
            }
        }
    }

    public static void RestoreBackupFiles(string folderPath)
    {
        string backupPathLevelsdat = GetFilePath(Path.Combine(folderPath, "backup"), "LEVELS.DAT");
        if (backupPathLevelsdat != null)
        {
            if (Directory.Exists(folderPath))
            {
                File.Copy(Path.Combine(folderPath, "LEVELS.DAT"), Path.Combine(backupPathLevelsdat, "LEVELS.DAT"), true);
            }
        }

        string backupPathLevelstab = GetFilePath(Path.Combine(folderPath, "backup"), "LEVELS.TAB");
        if (backupPathLevelstab != null)
        {
            if (Directory.Exists(folderPath))
            {
                File.Copy(Path.Combine(folderPath, "LEVELS.TAB"), Path.Combine(backupPathLevelstab, "LEVELS.TAB"), true);
            }
        }
    }

    public static string? GetFilePath(string gameFolder, string fileName)
    {
        gameFolder = Path.GetDirectoryName(gameFolder);
        if (Directory.Exists(gameFolder))
        {
            string[] files = Directory.GetFiles(gameFolder);
            return files?.Where(f => f.EndsWith(fileName, System.StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        }
        return null;
    }

    public static string? GetFolderPath(string gameFolder, string folderName)
    {
        gameFolder = Path.GetDirectoryName(gameFolder);
        if (Directory.Exists(gameFolder) && gameFolder.EndsWith(folderName, System.StringComparison.CurrentCultureIgnoreCase))
        {
            return gameFolder;
        }
        return null;
    }
}
