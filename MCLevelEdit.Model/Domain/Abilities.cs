namespace MCLevelEdit.Model.Domain
{
    public class Abilities
    {
        public bool StartsWith { get; set; }
        public bool CannotHave { get; set; }
        public bool WillLearnIfYouDo { get; set; }
        public bool CarriesCannotUse { get; set; }

        public Abilities() { }

        public Abilities(byte value1, byte value2) {
            StartsWith = value1 == 1 && value2 == 1;
            CannotHave = value1 == 0 && value2 == 0;
            WillLearnIfYouDo = value1 == 0 && value2 == 1;
            CarriesCannotUse = value1 == 1 && value2 == 0;
        }
    }
}
