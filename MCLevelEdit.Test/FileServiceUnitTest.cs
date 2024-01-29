using MCLevelEdit.Infrastructure.Adapters;

namespace MCLevelEdit.Test
{
    public class FileServiceTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [TestCase(@"Resources\data_01ea15.dat")]
        [TestCase(@"Resources\data_1c0000.dat")]
        public void LoadMapFromFile(string path)
        {
            var service = new FileAdapter();
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), path);
            var map = service.LoadMap(fullPath);

            Assert.IsNotNull(map);
            Assert.IsTrue(map.Entities?.Any());
        }

        [TestCase(@"Resources\data_01ea15.dat")]
        [TestCase(@"Resources\data_1c0000.dat")]
        public void SaveMapToFile(string path)
        {
            var service = new FileAdapter();
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), path);
            var testPath = Path.Combine(Directory.GetCurrentDirectory(), "test.dat");
            var levfileBefore = File.ReadAllBytes(fullPath);

            var map = service.LoadMap(fullPath);

            Assert.IsNotNull(map);
            Assert.IsTrue(map.Entities?.Any());

            bool result = service.SaveMap(map, testPath);
            var levfileAfter = File.ReadAllBytes(testPath);

            Assert.IsTrue(result);
            Assert.IsTrue(ByteArrayCompare(levfileBefore, levfileAfter));
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