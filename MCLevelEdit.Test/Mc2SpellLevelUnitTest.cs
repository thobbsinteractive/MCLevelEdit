using MCLevelEdit.ViewModels;

namespace MCLevelEdit.Test
{
    public class Mc2SpellLevelTests
    {
        [TestCase(0, 0)]
        [TestCase(1, 1)]
        [TestCase(2, 2)]
        [TestCase(3, 2)]
        [TestCase(255, 2)]
        public void EditedLevelIsLimitedToTwo(int typed, int expected)
        {
            var spell = new Mc2SpellViewModel(0, "Fireball", 0, 0, 0);
            spell.Level = (byte)typed;

            Assert.That(spell.Level, Is.EqualTo(expected));
            Assert.That(spell.LevelRaw, Is.EqualTo(expected));
        }

        [Test]
        public void OutOfRangeLevelFromALevelFileIsKeptUntilEdited()
        {
            // Some of the game's own levels hold 3, 4 or 255; saving an unedited level must not change them
            var spell = new Mc2SpellViewModel(0, "Fireball", 0, 255, 0);

            Assert.That(spell.LevelRaw, Is.EqualTo(255));
        }
    }
}
