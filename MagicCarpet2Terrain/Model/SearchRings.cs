using System.Collections.Generic;

namespace MagicCarpet2Terrain.Model
{
    /// <summary>
    /// The rings of squares around a centre, nearest first, that the games change the ground in (remc2
    /// LoadSearchData_101C0, remc1 InitSearchRings_11540): DATA/SEARCH.DAT (unpacked) is a 32 x 32 map of ring
    /// numbers, ring 0 the centre.
    /// </summary>
    public class SearchRings
    {
        public const int GRID_SIZE = 32;
        private const int RING_COUNT = 32;

        // The offsets of all rings in turn, and where each ring starts and how many it has
        private readonly List<(int Dx, int Dy)> _offsets = new List<(int, int)>();
        private readonly int[] _start = new int[RING_COUNT + 1];
        private readonly int[] _count = new int[RING_COUNT + 1];

        public static SearchRings Load(byte[] grid)
        {
            var rings = new SearchRings();
            int originX = 0, originY = 0;
            for (int ring = 0; ring < RING_COUNT; ring++)
            {
                rings._start[ring] = rings._offsets.Count;
                for (int y = 0; y < GRID_SIZE; y++)
                    for (int x = 0; x < GRID_SIZE; x++)
                    {
                        if (grid[y * GRID_SIZE + x] != ring)
                            continue;
                        if (ring == 0 && rings._offsets.Count == 0)
                        {
                            originX = x;
                            originY = y;
                        }
                        // stored as signed bytes
                        rings._offsets.Add(((sbyte)(x - originX), (sbyte)(y - originY)));
                        rings._count[ring]++;
                    }
            }
            rings._start[RING_COUNT] = rings._offsets.Count;
            return rings;
        }

        /// <summary>A search of the rings <paramref name="first"/> to <paramref name="last"/>.</summary>
        public Search Start(int first, int last) => new Search(this, first, last);

        /// <summary>
        /// remc2 GetNextSearchOffset_10130 / remc1 GetNextSearchOffset_114B0: the offsets in turn, the ring's count
        /// taken as it is (a ring of 0 runs into the next). The original stops before the last offset of the last ring.
        /// </summary>
        public class Search
        {
            private readonly SearchRings _rings;
            private readonly int _last;
            private int _ring;
            private int _position;
            private int _counter;
            private bool _done;

            internal Search(SearchRings rings, int first, int last)
            {
                _rings = rings;
                _last = last;
                _ring = first;
                _position = first >= 0 && first <= RING_COUNT ? rings._start[first] : rings._offsets.Count;
                _done = first < 0 || first >= RING_COUNT;
            }

            /// <returns>Whether there is an offset to change.</returns>
            public bool Next(out int dx, out int dy)
            {
                dx = 0;
                dy = 0;
                if (_done || _position >= _rings._offsets.Count)
                    return false;
                (dx, dy) = _rings._offsets[_position++];
                _counter++;
                if (_rings._count[_ring] > _counter)
                    return true;
                _counter = 0;
                _ring++;
                if (_ring > _last || _ring >= RING_COUNT)
                {
                    _done = true;
                    return false;
                }
                return true;
            }
        }
    }
}
