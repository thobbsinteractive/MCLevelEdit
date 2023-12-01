using Avalonia.Media;
using System;
using System.Linq;

namespace MCLevelEdit.DataModel
{        
    public enum Weather
    {
        Wind = 4
    }

    public class WeatherType : EntityType
    {
        private static ModelType[] _childTypes;

        public WeatherType(Weather weather) : base(TypeId.Weather, Color.FromRgb(0, 0, 255), ((int)weather), weather.ToString()) { }

        public override ModelType[] ChildTypes
        {
            get
            {
                if (_childTypes is null)
                {
                    _childTypes = Enum.GetValues(typeof(Weather))
                        .Cast<int>()
                        .Select(x => new ModelType() { Id = x, Name = Enum.GetName(typeof(Weather), x) })
                        .ToArray();
                }

                return _childTypes;
            }
        }
    }
}
