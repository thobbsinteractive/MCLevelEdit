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
                File.Copy(levelsdat, Path.Combine(backLevelsFolderPath, Path.ChangeExtension(levelsFileName, "DAT.BK")), false);
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
            string backupPathLevelsdat = GetFilePath(backLevelsFolderPath, levelsFileName);
            if (string.IsNullOrWhiteSpace(backupPathLevelsdat))
                throw new ArgumentNullException(nameof(backupPathLevelsdat));

            if (Directory.Exists(levelsFolderPath))
            {
                File.Copy(backupPathLevelsdat, Path.Combine(levelsFolderPath, levelsFileName), true);
            }


            string backupPathLevelstab = GetFilePath(backLevelsFolderPath, Path.ChangeExtension(levelsFileName, "TAB"));

            if (string.IsNullOrWhiteSpace(backupPathLevelstab))
                throw new ArgumentNullException(nameof(backupPathLevelstab));

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
