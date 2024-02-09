using Avalonia.Controls;
using Avalonia.Media;
using ReactiveUI;
using System.Windows.Input;

namespace MCLevelEdit.ViewModels
{
    public class AbilitiesViewModel : ReactiveObject
    {
        private bool _startsWith;
        private bool _cannotHave;
        private bool _willLearnIfYouDo;
        private bool _carriesCannotUse;
        private readonly int _spellNumber;
        private readonly string _spellName;

        public bool StartsWith
        {
            get => _startsWith;
            set { this.RaiseAndSetIfChanged(ref _startsWith, value); }
        }
        public bool CannotHave
        {
            get => _cannotHave;
            set { this.RaiseAndSetIfChanged(ref _cannotHave, value); }
        }
        public bool WillLearnIfYouDo
        {
            get => _willLearnIfYouDo;
            set { this.RaiseAndSetIfChanged(ref _willLearnIfYouDo, value); }
        }
        public bool CarriesCannotUse
        {
            get => _carriesCannotUse;
            set { this.RaiseAndSetIfChanged(ref _carriesCannotUse, value); }
        }

        public SolidColorBrush Foreground
        {
            get
            {
                var brush = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                if (WillLearnIfYouDo)
                {
                    brush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                }
                return brush;
            }
        }
        public SolidColorBrush Background
        {
            get
            {
                var brush = new SolidColorBrush(Color.FromRgb(50, 50, 50));
                if (StartsWith || CarriesCannotUse)
                {
                    brush = new SolidColorBrush(Color.FromRgb(128, 128, 128));
                }
                return brush;
            }
        }
        public string SpellNumber
        {
            get
            {
                string content = string.Empty;
                if (StartsWith || WillLearnIfYouDo || CarriesCannotUse)
                {
                    content = _spellNumber.ToString();
                }
                return content;
            }
        }

        public string ToolTip
        {
            get
            {
                var tip = $"{_spellName}: Cannot have";
                
                if (StartsWith)
                {
                    tip = $"{_spellName}: Starts With";
                }
                else if (WillLearnIfYouDo)
                {
                    tip = $"{_spellName}: Will learn if you do";
                }
                else if (CarriesCannotUse)
                {
                    tip = $"{_spellName}: Carries, but cannot use";
                }
                return tip;
            }
        }

        public bool IsVisible
        {
            get
            {
                return CarriesCannotUse;
            }
        }

        public ICommand ChangeSpellCommand { get; }

        public AbilitiesViewModel() 
        {
            ChangeSpellCommand = ReactiveCommand.Create(() =>
            {
                SetNext();
            });
        }

        public AbilitiesViewModel((byte, byte) values, int spellNumber, string spellName) : this(values.Item1, values.Item2, spellNumber, spellName) { }

        public AbilitiesViewModel(byte value1, byte value2, int spellNumber, string spellName) : this() 
        {
            StartsWith = value1 == 1 && value2 == 1;
            CannotHave = value1 == 0 && value2 == 0;
            WillLearnIfYouDo = value1 == 0 && value2 == 1;
            CarriesCannotUse = value1 == 1 && value2 == 0;
            _spellNumber = spellNumber;
            _spellName = spellName;
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
            this.RaisePropertyChanged(nameof(Foreground));
            this.RaisePropertyChanged(nameof(Background));
            this.RaisePropertyChanged(nameof(SpellNumber));
            this.RaisePropertyChanged(nameof(ToolTip));
            this.RaisePropertyChanged(nameof(IsVisible));
        }
    }
}
