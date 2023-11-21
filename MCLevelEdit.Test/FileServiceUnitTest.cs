using MCLevelEdit.Services;

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
            var service = new FileService();
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), path);
            var map = service.LoadMapFromFile(fullPath);
            Assert.Pass();
        }
    }
}