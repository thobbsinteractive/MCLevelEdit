namespace MCLevelEdit.Application.Utils;

public class FileUtils
{
    public static void DeleteExistingFiles(string folderPath, string levelsFileName = "LEVELS.DAT")
    {
        string levelsdat = GetFilePath(folderPath, levelsFileName);
        if (levelsdat != null)
        {
            FileInfo fInfo = new FileInfo(levelsdat);
            if (fInfo.IsReadOnly)
            {
                fInfo.IsReadOnly = false;
            }
            File.Delete(levelsdat);
        }
        string levelstab = GetFilePath(folderPath, Path.ChangeExtension(levelsFileName, "TAB"));
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

    public static void SetFilesToReadonly(string folderPath, string levelsFileName = "LEVELS.DAT")
    {
        string levelsdat = GetFilePath(folderPath, levelsFileName);
        if (levelsdat != null)
        {
            FileInfo fInfo = new FileInfo(levelsdat);
            fInfo.IsReadOnly = true;

        }
        string levelstab = GetFilePath(folderPath, Path.ChangeExtension(levelsFileName, "TAB"));
        if (levelstab != null)
        {
            FileInfo fInfo = new FileInfo(levelstab);
            fInfo.IsReadOnly = true;
        }
    }

    public static bool CopyBackupFiles(string folderPath, string backLevelsFolderPath, string levelsFileName = "LEVELS.DAT")
    {
        try
        {
            string levelsdat = GetFilePath(folderPath, levelsFileName);

            if (string.IsNullOrWhiteSpace(levelsdat))
                throw new ArgumentNullException(nameof(levelsdat));

            if (!Directory.Exists(backLevelsFolderPath))
            {
                Directory.CreateDirectory(backLevelsFolderPath);
            }

            if (Directory.Exists(backLevelsFolderPath))
            {
                var backupLevelsDat = Path.Combine(backLevelsFolderPath, Path.ChangeExtension(levelsFileName, "DAT.BK"));

                if (File.Exists(backupLevelsDat))
                    return true;

                File.Copy(levelsdat, backupLevelsDat, false);
            }

            string levelstab = GetFilePath(folderPath, Path.ChangeExtension(levelsFileName, "TAB"));

            if (Directory.Exists(backLevelsFolderPath))
            {
                File.Copy(levelstab, Path.Combine(backLevelsFolderPath, Path.ChangeExtension(levelsFileName, "TAB.BK")), false);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error Copying Files: " + ex.ToString());
            return false;
        }
        return true;
    }

    public static bool RestoreBackupFiles(string levelsFolderPath, string backLevelsFolderPath, string levelsFileName = "LEVELS.DAT")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(levelsFolderPath))
                throw new ArgumentNullException(nameof(levelsFolderPath));

            string backupPathLevelsdat = GetFilePath(backLevelsFolderPath, Path.ChangeExtension(levelsFileName, "DAT.BK"));
            if (string.IsNullOrWhiteSpace(backLevelsFolderPath))
                throw new ArgumentNullException(nameof(backLevelsFolderPath));

            if (Directory.Exists(levelsFolderPath))
            {
                File.Copy(backupPathLevelsdat, Path.Combine(levelsFolderPath, levelsFileName), true);
            }

            string backupPathLevelstab = GetFilePath(backLevelsFolderPath, Path.ChangeExtension(levelsFileName, "TAB.BK"));

            if (Directory.Exists(levelsFolderPath))
            {
                File.Copy(backupPathLevelstab, Path.Combine(levelsFolderPath, Path.ChangeExtension(levelsFileName, "TAB")), true);
            }

        } catch(Exception ex) 
        {
            Console.WriteLine("Error Restoring Files: " + ex.ToString());
            return false;
        }
        return true;
    }

    public static string? GetFilePath(string gameFolder, string fileName)
    {
        gameFolder = Path.GetDirectoryName(gameFolder);
        if (Directory.Exists(gameFolder))
        {
            string[] files = Directory.GetFiles(gameFolder);
            return files?.Where(f => Path.GetFileName(f).Equals(fileName, System.StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
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
