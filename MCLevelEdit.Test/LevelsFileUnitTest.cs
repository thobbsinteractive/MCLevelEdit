using MCLevelEdit.Application.Services;
using MCLevelEdit.Model.Domain;

namespace MCLevelEdit.Test
{
    public class LevelsFileTests
    {
        [TestCase(GameTarget.MagicCarpet1, "LEVELS.DAT")]
        [TestCase(GameTarget.HiddenWorlds, "DDLEVELS.DAT")]
        public void EachDosGameHasItsOwnLevelsFile(GameTarget game, string expected)
        {
            Assert.That(LevelsFile.For(game), Is.EqualTo(expected));
        }

        [Test]
        public async Task HiddenWorldsBackupAndRestoreLeaveMagicCarpetLevelsAlone()
        {
            // Both games' levels live in the same LEVELS folder and can share a backup folder
            var root = Path.Combine(Path.GetTempPath(), "MCLevelEdit.Test." + Guid.NewGuid().ToString("N"));
            var levels = Path.Combine(root, "LEVELS") + Path.DirectorySeparatorChar;
            var backup = Path.Combine(root, "BACKUP") + Path.DirectorySeparatorChar;
            Directory.CreateDirectory(levels);
            try
            {
                File.WriteAllText(Path.Combine(levels, "LEVELS.DAT"), "mc1 dat");
                File.WriteAllText(Path.Combine(levels, "LEVELS.TAB"), "mc1 tab");
                File.WriteAllText(Path.Combine(levels, "DDLEVELS.DAT"), "hw dat");
                File.WriteAllText(Path.Combine(levels, "DDLEVELS.TAB"), "hw tab");

                var service = new GameService(null!);
                Assert.That(await service.BackupLevelFiles(levels, backup, LevelsFile.HIDDEN_WORLDS), Is.True);
                Assert.That(Directory.GetFiles(backup).Select(Path.GetFileName).OrderBy(n => n),
                    Is.EqualTo(new[] { "DDLEVELS.DAT.BK", "DDLEVELS.TAB.BK" }));

                // A custom Hidden Worlds level replaces DDLEVELS, then the original comes back
                File.WriteAllText(Path.Combine(levels, "DDLEVELS.DAT"), "custom");
                Assert.That(await service.RestoringLevelFiles(backup, new[] { levels }, LevelsFile.HIDDEN_WORLDS), Is.True);

                Assert.That(File.ReadAllText(Path.Combine(levels, "DDLEVELS.DAT")), Is.EqualTo("hw dat"));
                Assert.That(File.ReadAllText(Path.Combine(levels, "DDLEVELS.TAB")), Is.EqualTo("hw tab"));
                Assert.That(File.ReadAllText(Path.Combine(levels, "LEVELS.DAT")), Is.EqualTo("mc1 dat"));
                Assert.That(File.ReadAllText(Path.Combine(levels, "LEVELS.TAB")), Is.EqualTo("mc1 tab"));
            }
            finally
            {
                foreach (var f in Directory.GetFiles(root, "*", SearchOption.AllDirectories))
                    File.SetAttributes(f, FileAttributes.Normal);
                Directory.Delete(root, true);
            }
        }
    }
}
