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
        public void LoadMapFromFile(string path)
        {
            var service = new FileAdapter();
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), path);
            var map = service.LoadMapAsync(fullPath).Result;

            Assert.IsNotNull(map);
            Assert.IsTrue(map.Entities?.Any());
        }
    }
}