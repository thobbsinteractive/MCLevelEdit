namespace MCLevelEdit.Infrastructure.Interfaces;

public interface IPackagePort
{
    public Task<bool> PackageFilesAsync(string[] fullFilePaths, string outputDirectory);
    public bool PackageFiles(string[] fullFilePaths, string outputDirectory);
}
