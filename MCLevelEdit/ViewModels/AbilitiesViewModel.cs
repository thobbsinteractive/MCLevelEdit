namespace MCLevelEdit.ViewModels
{
    public class AbilitiesViewModel
    {
        public bool StartsWith { get; set; }
        public bool CannotHave { get; set; }
        public bool WillLearnIfYouDo { get; set; }
        public bool CarriesCannotUse { get; set; }

        public AbilitiesViewModel() { }

        public AbilitiesViewModel((byte, byte) values) : this(values.Item1, values.Item2) { }

        public AbilitiesViewModel(byte value1, byte value2) {
            StartsWith = value1 == 1 && value2 == 1;
            CannotHave = value1 == 0 && value2 == 0;
            WillLearnIfYouDo = value1 == 0 && value2 == 1;
            CarriesCannotUse = value1 == 1 && value2 == 0;
        }

        public (byte, byte) GetBytes()
        {
            if (StartsWith)
                return new (1, 1);

            if (CannotHave)
                return new (0, 0);

            if (WillLearnIfYouDo)
                return (0, 1);

            if (CarriesCannotUse)
                return new (1, 0);

            return new (0, 1);
        }
    }
}
