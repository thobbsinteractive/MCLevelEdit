using MCLevelEdit.Infrastructure.Adapters;

namespace MCLevelEdit.Test
{
    public class FileServiceTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [TestCase(@"Resources\data_000e0f.DAT")]
        public void LoadMapFromFile(string path)
        {
            var service = new FileAdapter();
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), path);
            var map = service.LoadMap(fullPath);
            Assert.Pass();
        }
    }
}