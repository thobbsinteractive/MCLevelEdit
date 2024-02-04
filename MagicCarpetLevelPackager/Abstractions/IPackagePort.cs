using System.Threading.Tasks;

namespace MagicCarpetLevelPackager.Abstractions
{
    public interface IPackagePort
    {
        Task<bool> PackageFilesAsync(string[] fullFilePaths, string outputDirectory);
        bool PackageFiles(string[] fullFilePaths, string outputDirectory);
    }
}
