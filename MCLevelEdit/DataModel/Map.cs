﻿using CommunityToolkit.Mvvm.ComponentModel;

namespace MCLevelEdit.DataModel
{
    public class Map : ObservableObject
    {
        public static ushort Size = 256;

        private Square[,] _squares;

        public Square[,] Squares
        {
            get { return _squares; }
            set
            {
                SetProperty(ref _squares, value);
            }
        }

        public Map(Square[, ] squares, ushort size)
        {
            _squares = squares;
            Size = size;
        }
    }
}
