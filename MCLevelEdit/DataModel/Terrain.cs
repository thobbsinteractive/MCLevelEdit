namespace MCLevelEdit.DataModel
{
    public class Terrain
    {
        private short[] _mapEntityIndex_15B4E0;
        private byte[] _mapHeightmap_11B4E0;
        private byte[] _mapAngle_13B4E0;
        private byte[] _mapTerrainType_10B4E0;
        private byte[] _mapShading_12B4E0;

        public short[] MapEntityIndex_15B4E0
        {
            get { return _mapEntityIndex_15B4E0; }
            set { _mapEntityIndex_15B4E0 = value; }
        }

        public byte[] MapHeightmap_11B4E0
        {
            get { return _mapHeightmap_11B4E0; }
            set { _mapHeightmap_11B4E0 = value; }
        }

        public byte[] MapAngle_13B4E0
        {
            get { return _mapAngle_13B4E0; }
            set { _mapAngle_13B4E0 = value; }
        }

        public byte[] MapTerrainType_10B4E0
        {
            get { return _mapTerrainType_10B4E0; }
            set { _mapTerrainType_10B4E0 = value; }
        }

        public byte[] MapShading_12B4E0
        {
            get { return _mapShading_12B4E0; }
            set { _mapShading_12B4E0 = value; }
        }
    }
}
