using CommunityToolkit.Mvvm.ComponentModel;
using MagicCarpet2Terrain.Model;

namespace MCLevelEdit.ViewModels
{
    public class TerrainGenerationParamsViewModel : ObservableObject
    {
        private MapType _mapType = MapType.Day;
        private ushort _seed = 0;
        private ushort _offset = 0;
        private uint _raise = 0;
        private ushort _gnarl = 0;
        private ushort _river = 0;
        private ushort _lriver = 0;
        private byte _source = 0;
        private ushort _snLin = 0;
        private byte _snFlt = 0;
        private byte _bhLin = 0;
        private byte _bhFlt = 0;
        private byte _rkSte = 0;

        public MapType MapType
        {
            get { return _mapType; }
            set { SetProperty(ref _mapType, value); }
        }

        public ushort Seed
        {
            get { return _seed; }
            set { SetProperty(ref _seed, value); }
        }

        public ushort Offset
        {
            get { return _offset; }
            set { SetProperty(ref _offset, value); }
        }

        public uint Raise
        {
            get { return _raise; }
            set { SetProperty(ref _raise, value); }
        }

        public ushort Gnarl
        {
            get { return _gnarl; }
            set { SetProperty(ref _gnarl, value); }
        }

        public ushort River
        {
            get { return _river; }
            set { SetProperty(ref _river, value); }
        }

        public ushort LRiver
        {
            get { return _lriver; }
            set { SetProperty(ref _lriver, value); }
        }

        public byte Source
        {
            get { return _source; }
            set { SetProperty(ref _source, value); }
        }

        public ushort SnLin
        {
            get { return _snLin; }
            set { SetProperty(ref _snLin, value); }
        }

        public byte SnFlt
        {
            get { return _snFlt; }
            set { SetProperty(ref _snFlt, value); }
        }

        public byte BhLin
        {
            get { return _bhLin; }
            set { SetProperty(ref _bhLin, value); }
        }

        public byte BhFlt
        {
            get { return _bhFlt; }
            set { SetProperty(ref _bhFlt, value); }
        }

        public byte RkSte
        {
            get { return _rkSte; }
            set { SetProperty(ref _rkSte, value); }
        }

        public void SetParameters(TerrainGenerationParamsViewModel terrainGenerationParameters)
        {
            MapType = terrainGenerationParameters.MapType;
            Seed = terrainGenerationParameters.Seed;
            Offset = terrainGenerationParameters.Offset;
            Raise = terrainGenerationParameters.Raise;
            Gnarl = terrainGenerationParameters.Gnarl;
            River = terrainGenerationParameters.River;
            LRiver = terrainGenerationParameters.LRiver;
            Source = terrainGenerationParameters.Source;
            SnLin = terrainGenerationParameters.SnLin;
            SnFlt = terrainGenerationParameters.SnFlt;
            BhLin = terrainGenerationParameters.BhLin;
            BhFlt = terrainGenerationParameters.BhFlt;
            RkSte = terrainGenerationParameters.RkSte;
        }
    }
}
