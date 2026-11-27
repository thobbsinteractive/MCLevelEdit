using MCLevelEdit.Utils;

namespace MCLevelEdit.Test
{
    public class PickerStartFolderTests
    {
        private string _root = null!;
        private string Levels => Path.Combine(_root, "LEVELS");

        [SetUp]
        public void CreateFolders()
        {
            _root = Path.Combine(Path.GetTempPath(), "MCLevelEdit.Test." + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Levels);
            File.WriteAllText(Path.Combine(Levels, "LEVELS.DAT"), "");
        }

        [TearDown]
        public void DeleteFolders() => Directory.Delete(_root, true);

        [TestCase(false)]
        [TestCase(true)]
        public void AnExistingFolderStartsInItself(bool isFolder)
        {
            Assert.That(PickerStartFolder.FindExistingFolder(Levels, isFolder), Is.EqualTo(Levels));
            // with the trailing backslash the settings defaults use
            Assert.That(PickerStartFolder.FindExistingFolder(Levels + Path.DirectorySeparatorChar, isFolder), Is.EqualTo(Levels));
        }

        [Test]
        public void AFileStartsInItsFolder()
        {
            Assert.That(PickerStartFolder.FindExistingFolder(Path.Combine(Levels, "LEVELS.DAT")), Is.EqualTo(Levels));
        }

        [Test]
        public void AFileNotSavedYetStartsInItsFolder()
        {
            // e.g. the Package Levels output file before the first package
            Assert.That(PickerStartFolder.FindExistingFolder(Path.Combine(Levels, "NEW.DAT")), Is.EqualTo(Levels));
        }

        [Test]
        public void AFolderThatDoesNotExistHasNoStartFolder()
        {
            // e.g. a backup folder that has not been created: not its parent
            Assert.That(PickerStartFolder.FindExistingFolder(Path.Combine(Levels, "BACKUP"), isFolder: true), Is.Null);
            Assert.That(PickerStartFolder.FindExistingFolder(Path.Combine(Levels, "BACKUP") + Path.DirectorySeparatorChar, isFolder: true), Is.Null);
        }

        [Test]
        public void AFileInAFolderThatDoesNotExistHasNoStartFolder()
        {
            // e.g. a game that is not installed
            Assert.That(PickerStartFolder.FindExistingFolder(Path.Combine(Levels, "DOSBOX", "DOSBox.exe")), Is.Null);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void NoPathHasNoStartFolder(string? path)
        {
            Assert.That(PickerStartFolder.FindExistingFolder(path), Is.Null);
            Assert.That(PickerStartFolder.FindExistingFolder(path, isFolder: true), Is.Null);
        }
    }
}
