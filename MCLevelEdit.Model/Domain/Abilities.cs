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

        public byte[] GetBytes()
        {
            if (StartsWith)
                return new byte[] { 1 , 1 };

            if (CannotHave)
                return new byte[] { 0, 0 };

            if (WillLearnIfYouDo)
                return new byte[] { 0, 1 };

            if (CarriesCannotUse)
                return new byte[] { 1, 0 };

            return new byte[] { 0, 1 };
        }
    }
}
