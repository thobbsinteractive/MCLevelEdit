using MCLevelEdit.Application.Model;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using Splat;
using System;
using System.IO;
using System.Windows.Input;

namespace MCLevelEdit.ViewModels
{
    public class UnpackLevelsViewModel : ReactiveObject, IEnableLogger
    {
        private bool _canUnpack = true;
        public ICommand UnpackLevelsCommand { get; }
        public ICommand SetDefaultsCommand { get; }

        public string LevelsDatPath { get; set; }
        public string OutputPath { get; set; }

        public bool CanUnpack
        {
            get => _canUnpack;
            set => this.RaiseAndSetIfChanged(ref _canUnpack, value);
        }

        public UnpackLevelsViewModel(EventAggregator<object> eventAggregator)
        {
            LevelsDatPath = @"C:\Program Files (x86)\GOG Galaxy\Games\Magic Carpet Plus\CARPET.CD\LEVELS\LEVELS.DAT";
            OutputPath = @"C:\Program Files (x86)\GOG Galaxy\Games\Magic Carpet Plus\CARPET.CD\LEVELS\Extracted\";

            SetDefaultsCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                LevelsDatPath = @"C:\Program Files (x86)\GOG Galaxy\Games\Magic Carpet Plus\CARPET.CD\LEVELS\LEVELS.DAT";
                OutputPath = @"C:\Program Files (x86)\GOG Galaxy\Games\Magic Carpet Plus\CARPET.CD\LEVELS\Extracted\";
                this.RaisePropertyChanged(nameof(LevelsDatPath));
                this.RaisePropertyChanged(nameof(OutputPath));
            });

            UnpackLevelsCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                try
                {
                    CanUnpack = false;

                    if (!Directory.Exists(OutputPath))
                    {
                        Directory.CreateDirectory(OutputPath);
                    }

                    var errorCode = UnpackFile(LevelsDatPath, OutputPath);
                    if (errorCode != 0)
                        throw new Exception($"Unknown Error, code {errorCode}");
                }
                catch (Exception ex)
                {
                    this.Log().Error(ex, $"Error unpacking files!");
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", $"Error unpacking Levels! Please check and validate your paths: {ex.Message}", ButtonEnum.Ok, Icon.Warning);
                    await box.ShowAsync();
                }
                finally
                {
                    CanUnpack = true;
                }
            });
        }

        public int UnpackFile(string inputPath, string outputFolder)
        {
            uint MAX_BUF_SIZE = 0x1E00000;

            if (!Directory.Exists(outputFolder))
                throw new ArgumentException($"Output directory not found: {outputFolder}", nameof(outputFolder));

            if (!File.Exists(inputPath))
                throw new ArgumentException($"File not found {inputPath}", nameof(inputPath));

            var rncProPack = new RncProPackDotNet.RncProPack();
            var vars = rncProPack.InitVars();

            if (vars.Method == 1)
            {
                if (vars.DictSize > 0x8000)
                    vars.DictSize = 0x8000;
                vars.MaxMatches = 0x1000;
            }
            else if (vars.Method == 2)
            {
                if (vars.DictSize > 0x1000)
                    vars.DictSize = 0x1000;
                vars.MaxMatches = 0xFF;
            }

            using (FileStream inFile = new FileStream(inputPath, FileMode.Open, FileAccess.Read))
            {
                vars.FileSize = (uint)(inFile.Length - vars.ReadStartOffset);
                inFile.Seek(vars.ReadStartOffset, SeekOrigin.Begin);
                vars.Input = new byte[vars.FileSize];
                inFile.Read(vars.Input, 0, (int)vars.FileSize);
            }

            vars.Output = new byte[MAX_BUF_SIZE];
            vars.Temp = new byte[MAX_BUF_SIZE];

            return rncProPack.DoSearch(ref vars, vars.FileSize, vars.PuseMode == 'e', OutputPath);
        }
    }
}
