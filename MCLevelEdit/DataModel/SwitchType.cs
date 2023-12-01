using Avalonia.Media;
using System;
using System.Linq;

namespace MCLevelEdit.DataModel
{
    public enum Switch
    {
        HiddenInside = 0,
        HiddenOutside = 1,
        HiddenInsideRe = 2,
        HiddenOutsideRe = 3,
        OnVictory = 4,
        DeathInside = 5,
        DeathOutside = 6,
        DeathInsideRe = 7,
        DeathOutsideRe = 8,
        ObviousInside = 9,
        ObviousOutside = 10,
        ObviousInsideRe = 11,
        ObviousOutsideRe = 12,
        Dragon = 13,
        Vulture = 14,
        Bee = 15,
        None = 16,
        Archer = 17,
        Crab = 18,
        Kraken = 19,
        TrollApe = 20,
        Griffin = 21,
        Unknown1 = 22,
        Unknown2 = 23,
        Genie = 24,
        Unknown3 = 25,
        Unknown4 = 26,
        Unknown5 = 27,
        Unknown6 = 28,
        Wyvern = 29,
        CreatureAll = 30
    }

    public class SwitchType : EntityType
    {
        private static ModelType[] _childTypes;

        public SwitchType(Switch gameSwitch) : base(TypeId.Switch, Color.FromRgb(255, 255, 255), ((int)gameSwitch), gameSwitch.ToString()) { }

        public override ModelType[] ChildTypes
        {
            get
            {
                if (_childTypes is null)
                {
                    _childTypes = Enum.GetValues(typeof(Switch))
                        .Cast<int>()
                        .Select(x => new ModelType() { Id = x, Name = Enum.GetName(typeof(Switch), x) })
                        .ToArray();
                }

                return _childTypes;
            }
        }
    }
}
