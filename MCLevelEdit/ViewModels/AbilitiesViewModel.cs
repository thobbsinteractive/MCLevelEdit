﻿using Avalonia.Media;
using Avalonia.Media.Imaging;
using ReactiveUI;
using System;
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
        private readonly Bitmap _blackIcon;
        private readonly Bitmap _whiteIcon;

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
        public bool CarriesCannotUse //Might be hardcoded. Is the same as CannotHave in testing
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

        public Bitmap Icon
        {
            get
            {
                if (WillLearnIfYouDo)
                {
                    return _whiteIcon;
                }
                return _blackIcon;
            }
        }

        public ICommand ChangeSpellCommand { get; }

        public event EventHandler SpellsUpdatedEvent;

        public AbilitiesViewModel() 
        {
            ChangeSpellCommand = ReactiveCommand.Create(() =>
            {
                SetNext();
            });
        }

        public AbilitiesViewModel((byte, byte) values, int spellNumber, string spellName, Bitmap blackIcon, Bitmap whiteIcon) : this ()
        {
            StartsWith = values.Item1 == 1 && values.Item2 == 1;
            CannotHave = values.Item1 == 0 && values.Item2 == 0;
            WillLearnIfYouDo = values.Item1 == 0 && values.Item2 == 1;
            CarriesCannotUse = values.Item1 == 1 && values.Item2 == 0;
            _spellNumber = spellNumber;
            _spellName = spellName;
            _blackIcon = blackIcon;
            _whiteIcon = whiteIcon;
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
            this.RaisePropertyChanged(nameof(Icon));

            SpellsUpdatedEvent?.Invoke(this, new EventArgs());
        }
    }
}
