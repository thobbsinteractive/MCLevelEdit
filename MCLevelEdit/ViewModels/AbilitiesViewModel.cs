using ReactiveUI;
using System.Windows.Input;

namespace MCLevelEdit.ViewModels
{
    public class AbilitiesViewModel : ReactiveObject
    {

        public bool StartsWith { get; set; }
        public bool CannotHave { get; set; }
        public bool WillLearnIfYouDo { get; set; }
        public bool CarriesCannotUse { get; set; }

        public ICommand ChangeSpellCommand { get; }

        public AbilitiesViewModel() { }

        public AbilitiesViewModel((byte, byte) values) : this(values.Item1, values.Item2) { }

        public AbilitiesViewModel(byte value1, byte value2) {
            StartsWith = value1 == 1 && value2 == 1;
            CannotHave = value1 == 0 && value2 == 0;
            WillLearnIfYouDo = value1 == 0 && value2 == 1;
            CarriesCannotUse = value1 == 1 && value2 == 0;

            ChangeSpellCommand = ReactiveCommand.Create(() =>
            {
                SetNext();
            });
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

        public void SetNext()
        {
            if (StartsWith)
            {
                StartsWith = false;
                WillLearnIfYouDo = true;
                CarriesCannotUse = false;
                CannotHave = false;
            }
            else if (WillLearnIfYouDo)
            {
                StartsWith = false;
                WillLearnIfYouDo = false;
                CarriesCannotUse = true;
                CannotHave = false;
            }
            else if (CarriesCannotUse)
            {
                StartsWith = false;
                WillLearnIfYouDo = false;
                CarriesCannotUse = false;
                CannotHave = true;
            }
            else if (CannotHave)
            {
                StartsWith = true;
                WillLearnIfYouDo = false;
                CarriesCannotUse = false;
                CannotHave = false;
            }
        }
    }
}
