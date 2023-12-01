using Avalonia.Media;
using System;
using System.Linq;

namespace MCLevelEdit.DataModel
{
    public enum Spawn
    {
        Flyer1 = 4,
        Flyer2 = 5,
        Flyer3 = 6,
        Flyer4 = 7,
        Flyer5 = 8,
        Flyer6 = 9,
        Flyer7 = 10,
        Flyer8 = 11
    }

    public class SpawnType : EntityType
    {
        private static ModelType[] _childTypes;

        public SpawnType(Spawn spawn) : base(TypeId.Spawn, Color.FromRgb(255,255,0), ((int)spawn), spawn.ToString()) { }

        public override ModelType[] ChildTypes
        {
            get
            {
                if (_childTypes is null)
                {
                    _childTypes = Enum.GetValues(typeof(Spawn))
                        .Cast<int>()
                        .Select(x => new ModelType() { Id = x, Name = Enum.GetName(typeof(Spawn), x) })
                        .ToArray();
                }

                return _childTypes;
            }
        }
    }
}
