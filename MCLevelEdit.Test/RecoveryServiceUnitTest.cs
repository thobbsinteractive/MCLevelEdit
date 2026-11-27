using MCLevelEdit.Application.Services;
using MCLevelEdit.Infrastructure.Adapters;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.Model.Repository;

namespace MCLevelEdit.Test
{
    public class RecoveryServiceUnitTest
    {
        private string _folder = string.Empty;
        private Map? _previousMap;

        [SetUp]
        public void Setup()
        {
            _folder = Path.Combine(Path.GetTempPath(), "MCLevelEditRecoveryTest_" + Guid.NewGuid().ToString("N"));
            _previousMap = MapRepository.Map;
            MapRepository.Map = new FileAdapter().LoadMap(Path.Combine(Directory.GetCurrentDirectory(), @"Resources\data_01ea15.dat"));
            MapRepository.Map.FilePath = @"C:\Levels\MyLevel.DAT";
        }

        [TearDown]
        public void TearDown()
        {
            MapRepository.Map = _previousMap!;
            if (Directory.Exists(_folder))
                Directory.Delete(_folder, true);
        }

        [Test]
        public void SavedLevelIsFoundWithItsDetailsAndLoads()
        {
            var service = new RecoveryService(new FileAdapter(), _folder);
            service.RecoveryHandled();

            Assert.That(service.SaveRecovery("Autosave"), Is.True);

            // the next start
            var next = new RecoveryService(new FileAdapter(), _folder);
            var info = next.FindRecovery();
            Assert.That(info, Is.Not.Null);
            Assert.That(info!.OriginalFilePath, Is.EqualTo(@"C:\Levels\MyLevel.DAT"));
            Assert.That(info.Reason, Is.EqualTo("Autosave"));

            var loaded = new FileAdapter().LoadMap(info.FilePath);
            Assert.That(loaded.Entities.Count,
                Is.EqualTo(MapRepository.Map.Entities.Count));
        }

        [Test]
        public void RecoveryFromBeforeIsNotSavedOverUntilHandled()
        {
            var first = new RecoveryService(new FileAdapter(), _folder);
            first.RecoveryHandled();
            first.SaveRecovery("Crash");
            var written = File.GetLastWriteTimeUtc(Path.Combine(_folder, "Recovery.DAT"));

            var next = new RecoveryService(new FileAdapter(), _folder);
            Assert.That(next.SaveRecovery("Autosave"), Is.False);
            next.ClearRecovery();
            Assert.That(next.FindRecovery(), Is.Not.Null, "kept until the person decides");
            Assert.That(next.FindRecovery()!.Reason, Is.EqualTo("Crash"));
            Assert.That(File.GetLastWriteTimeUtc(Path.Combine(_folder, "Recovery.DAT")), Is.EqualTo(written));

            next.RecoveryHandled();
            Assert.That(next.SaveRecovery("Autosave"), Is.True);
        }

        [Test]
        public void DiscardSetsItAsideAndClearDeletes()
        {
            var service = new RecoveryService(new FileAdapter(), _folder);
            service.RecoveryHandled();
            service.SaveRecovery("Autosave");

            service.DiscardRecovery();
            Assert.That(service.FindRecovery(), Is.Null);
            Assert.That(File.Exists(Path.Combine(_folder, "Recovery_Discarded.DAT")), Is.True);

            service.SaveRecovery("Autosave");
            service.ClearRecovery();
            Assert.That(service.FindRecovery(), Is.Null);
        }
    }
}
