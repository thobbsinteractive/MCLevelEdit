using MCLevelEdit.Application.Utils;

namespace MCLevelEdit.Test
{
    public class RncUtilsTests
    {
        private static void WriteHeader(byte[] data, int at, byte method, uint packedSize)
        {
            data[at] = (byte)'R'; data[at + 1] = (byte)'N'; data[at + 2] = (byte)'C'; data[at + 3] = method;
            data[at + 8] = (byte)(packedSize >> 24);
            data[at + 9] = (byte)(packedSize >> 16);
            data[at + 10] = (byte)(packedSize >> 8);
            data[at + 11] = (byte)packedSize;
        }

        [Test]
        public void EndIsAfterTheLastBlockNotThePadding()
        {
            var data = new byte[10000];
            WriteHeader(data, 10, 1, 100);   // ends at 10 + 18 + 100 = 128
            WriteHeader(data, 200, 2, 300);  // ends at 200 + 18 + 300 = 518, then padding

            Assert.That(RncUtils.FindEndOfRncData(data), Is.EqualTo(518));
        }

        [Test]
        public void HeaderWhosePackedSizeRunsPastTheDataIsIgnored()
        {
            var data = new byte[1000];
            WriteHeader(data, 0, 1, 50);         // ends at 68
            WriteHeader(data, 300, 1, 0xFFFFFF); // not a real block

            Assert.That(RncUtils.FindEndOfRncData(data), Is.EqualTo(68));
        }

        [Test]
        public void UnknownMethodIsNotAHeader()
        {
            var data = new byte[1000];
            WriteHeader(data, 0, 1, 50);   // ends at 68
            WriteHeader(data, 300, 7, 50); // "RNC" text, but not a packed block

            Assert.That(RncUtils.FindEndOfRncData(data), Is.EqualTo(68));
        }

        [Test]
        public void NoBlocksSearchesEverything()
        {
            Assert.That(RncUtils.FindEndOfRncData(new byte[500]), Is.EqualTo(500));
        }

        [Test]
        public void ExtractedFilesAreNumberedInAddressOrder()
        {
            var folder = Path.Combine(Path.GetTempPath(), "MCLevelEdit.Test." + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
            try
            {
                // Written out of order, with hex addresses that Explorer's number-aware sort would misorder
                foreach (var name in new[] { "data_00A123.bin", "data_000008.bin", "data_001000.bin", "readme.txt", "007_data_000001.bin" })
                    File.WriteAllText(Path.Combine(folder, name), name);

                Assert.That(RncUtils.NumberExtractedFiles(folder), Is.EqualTo(3));

                var names = Directory.GetFiles(folder).Select(Path.GetFileName).OrderBy(n => n).ToArray();
                Assert.That(names, Is.EqualTo(new[] { "001_data_000008.bin", "002_data_001000.bin", "003_data_00A123.bin", "007_data_000001.bin", "readme.txt" }));
                // Contents move with the name
                Assert.That(File.ReadAllText(Path.Combine(folder, "003_data_00A123.bin")), Is.EqualTo("data_00A123.bin"));
            }
            finally
            {
                Directory.Delete(folder, true);
            }
        }
    }
}
