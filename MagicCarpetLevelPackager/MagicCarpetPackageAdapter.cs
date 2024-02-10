using MagicCarpetLevelPackager.Abstractions;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MagicCarpetLevelPackager
{
    public class MagicCarpetPackageAdapter : IPackagePort
    {
        public const int MAX_LEVELS = 70;
        public const int HEADER_SIZE_BYTES = 8;
        public const int LEVEL_FILE_SIZE = 38812;
        public const int LEVELS_TAB_FILE_SIZE = 4000;
        public const string LEVELS_DAT = "LEVELS.DAT";
        public const string LEVELS_TAB = "LEVELS.TAB";

        public Task<bool> PackageFilesAsync(string[] levelFilePaths, string outputDirectory)
        {
            return Task.Run<bool>(() => {
                return PackageFiles(levelFilePaths, outputDirectory);
            });
        }

        public bool PackageFiles(string[] levelFilePaths, string outputDirectory)
        {
            var existingLevelFiles = levelFilePaths.Where(f => File.Exists(f));
        
            if (existingLevelFiles is null || !existingLevelFiles.Any())
            {
                throw new ArgumentNullException(nameof(existingLevelFiles));
            }

            if (existingLevelFiles.Count() > MAX_LEVELS)
            {
                throw new ArgumentException($"Too many files. Maximum of {MAX_LEVELS}");
            }

            if (!Directory.Exists(outputDirectory))
            {
                throw new ArgumentNullException(nameof(outputDirectory));
            }

            // Define Header for DAT file
            byte[] levelsFileBytes = new byte[(LEVEL_FILE_SIZE * existingLevelFiles.Count()) + HEADER_SIZE_BYTES];

            WriteToArray(new byte[] { 0x42, 0x55, 0x4C, 0x4C, 0x46, 0x52, 0x4F, 0x47 }, levelsFileBytes, 0); // BULLFROG

            int fileIndex = HEADER_SIZE_BYTES;

            foreach (var filePath in existingLevelFiles)
            {
                var levelBytes = File.ReadAllBytes(filePath);
                WriteToArray(levelBytes, levelsFileBytes, fileIndex);
                fileIndex += LEVEL_FILE_SIZE;
                Console.WriteLine($"Added File: {filePath}");
            }

            File.WriteAllBytes(Path.Combine(outputDirectory, LEVELS_DAT), levelsFileBytes);

            int levelIndex = 8;
            fileIndex = 4;
            byte[] tabFileBytes = new byte[LEVELS_TAB_FILE_SIZE];
            WriteToArray(new byte[] { 0x08, 0x00, 0x00, 0x00 }, tabFileBytes, 0); // BULLFROG header means first entry is always byte 08

            foreach (var filePath in existingLevelFiles)
            {
                levelIndex += LEVEL_FILE_SIZE;
                WriteToArray(BitConverter.GetBytes(levelIndex), tabFileBytes, fileIndex);
                fileIndex += 4;
                Console.WriteLine($"Added File Address: {fileIndex}");
            }

            File.WriteAllBytes(Path.Combine(outputDirectory, LEVELS_TAB), tabFileBytes);
            return true;
        }

        private void WriteToArray(byte[] source, byte[] destination, int startIdx)
        {
            Buffer.BlockCopy(source, 0, destination, startIdx, source.Length);
        }
    }
}
