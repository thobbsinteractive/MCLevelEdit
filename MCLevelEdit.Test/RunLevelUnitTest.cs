using MCLevelEdit.Application.Services;
using MCLevelEdit.Infrastructure.Interfaces;
using MCLevelEdit.Model.Domain;
using Mc2Map = MCLevelEdit.Model.Domain.MC2.Mc2Map;

namespace MCLevelEdit.Test
{
    public class RunLevelTests
    {
        private class FakeSettingsPort : ISettingsPort
        {
            public Settings? Settings { get; set; }
            public string CurrentLevelFilePath { get; set; } = string.Empty;
            public bool SaveSettings(Settings settings) { Settings = settings; return true; }
            public Settings? LoadSettings() => Settings;
        }

        private static string ResourcePath(string name) => Path.Combine(Directory.GetCurrentDirectory(), "Resources", name);

        [Test]
        public void EachLevelRunsInItsOwnGame()
        {
            var map = new Map();
            Assert.That(map.Game, Is.EqualTo(GameTarget.MagicCarpet1));
            map.SetHiddenWorlds(true);
            Assert.That(map.Game, Is.EqualTo(GameTarget.HiddenWorlds));
            Assert.That(new Mc2Map().Game, Is.EqualTo(GameTarget.MagicCarpet2));
        }

        [TestCase(GameTarget.MagicCarpet1)]
        [TestCase(GameTarget.HiddenWorlds)]
        public async Task AMagicCarpet2LevelIsNeverPackedIntoTheDosGamesLevels(GameTarget game)
        {
            // Both DOSBox games are set up, and Magic Carpet is selected in Game Settings
            var root = Path.Combine(Path.GetTempPath(), "MCLevelEdit.Test." + Guid.NewGuid().ToString("N"));
            var levels = Path.Combine(root, "LEVELS") + Path.DirectorySeparatorChar;
            Directory.CreateDirectory(levels);
            try
            {
                var exe = Path.Combine(root, "DOSBox.exe");
                File.WriteAllText(exe, "");
                foreach (var file in new[] { "LEVELS.DAT", "LEVELS.TAB", "DDLEVELS.DAT", "DDLEVELS.TAB" })
                    File.WriteAllText(Path.Combine(levels, file), "original");
                var dos = new DosGameSettings { GameExeLocation = exe, GameLevelFolders = new[] { levels }, GameBackupFolder = Path.Combine(root, "BACKUP") + Path.DirectorySeparatorChar };
                var port = new FakeSettingsPort { Settings = new Settings { SelectedGame = GameTarget.MagicCarpet1, MagicCarpet1 = dos, HiddenWorlds = dos } };

                var ran = await new GameService(port).RunLevelFromSettings(new[] { ResourcePath("mc2_00169b.dat") }, game);

                Assert.That(ran, Is.False);
                foreach (var file in Directory.GetFiles(levels))
                    Assert.That(File.ReadAllText(file), Is.EqualTo("original"), Path.GetFileName(file));
            }
            finally
            {
                foreach (var f in Directory.GetFiles(root, "*", SearchOption.AllDirectories))
                    File.SetAttributes(f, FileAttributes.Normal);
                Directory.Delete(root, true);
            }
        }

        [Test]
        public async Task TheGivenGameIsRunNotTheSelectedOne()
        {
            // Magic Carpet is selected, but the level is for Magic Carpet 2 HD, whose exe is missing: it must not
            // fall back to running Magic Carpet
            var port = new FakeSettingsPort { Settings = new Settings { SelectedGame = GameTarget.MagicCarpet1, Mc2ExeLocation = @"Z:\missing\remc2.exe" } };

            var ran = await new GameService(port).RunLevelFromSettings(new[] { ResourcePath("mc2_00169b.dat") }, GameTarget.MagicCarpet2);

            Assert.That(ran, Is.False);
        }
    }
}
