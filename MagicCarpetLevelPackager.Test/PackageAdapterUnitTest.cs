using NUnit.Framework;

namespace MagicCarpetLevelPackager.Test
{
    public class PackageAdapterUnitTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [TestCase(@"Resources\data_1c0000.dat", @"Resources\data_01ea15.dat")]
        public void PackageAdapter(string path1, string path2)
        {
            string levelsdat = Path.Combine(Directory.GetCurrentDirectory(), "LEVELS.DAT");
            string levelstab = Path.Combine(Directory.GetCurrentDirectory(), "LEVELS.TAB");
            string levelsdatref = Path.Combine(Directory.GetCurrentDirectory(), @"Resources\LEVELS_REF.DAT");
            string levelstabref = Path.Combine(Directory.GetCurrentDirectory(), @"Resources\LEVELS_REF.TAB");

            if (File.Exists(levelsdat))
                File.Delete(levelsdat);

            if (File.Exists(levelstab))
                File.Delete(levelstab);

            var fullPaths = new List<string>
            {
                Path.Combine(Directory.GetCurrentDirectory(), path1),
                Path.Combine(Directory.GetCurrentDirectory(), path2)
            };

            var sut = new MagicCarpetPackageAdapter();

            var result = sut.PackageFiles(fullPaths.ToArray(), Directory.GetCurrentDirectory());

            Assert.That(result, Is.EqualTo(true));
            Assert.That(File.Exists(levelsdat));
            Assert.That(File.Exists(levelstab));
            Assert.That(ByteArrayCompare(File.ReadAllBytes(levelsdatref), File.ReadAllBytes(levelsdat)));
            //Assert.That(ByteArrayCompare(File.ReadAllBytes(levelstabref), File.ReadAllBytes(levelstab)));
        }

        private bool ByteArrayCompare(byte[] a1, byte[] a2)
        {
            if (a1.Length != a2.Length)
                return false;

            for (int i = 0; i < a1.Length; i++)
                if (a1[i] != a2[i])
                    return false;

            return true;
        }
    }
}
