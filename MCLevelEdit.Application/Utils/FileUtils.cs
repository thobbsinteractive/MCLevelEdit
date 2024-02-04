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
