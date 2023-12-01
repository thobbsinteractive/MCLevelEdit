using Avalonia.Media;
using System;
using System.Linq;

namespace MCLevelEdit.DataModel
{
    public enum Scenary
    {
        Tree = 0,
        StandingStone = 1,
        Dolmen = 2,
        BadStone = 3,
        Dome1 = 4,
        Dome2 = 5
    }

    public class ScenaryType : EntityType
    {
        private static ModelType[] _childTypes;

        public ScenaryType(Scenary scenary) : base(TypeId.Scenary, Color.FromRgb(0,255,0), ((int)scenary), scenary.ToString()) { }

        public override ModelType[] ChildTypes
        {
            get
            {
                if (_childTypes is null)
                {
                    _childTypes = Enum.GetValues(typeof(Scenary))
                        .Cast<int>()
                        .Select(x => new ModelType() { Id = x, Name = Enum.GetName(typeof(Scenary), x) })
                        .ToArray();
                }

                return _childTypes;
            }
        }
    }
}
