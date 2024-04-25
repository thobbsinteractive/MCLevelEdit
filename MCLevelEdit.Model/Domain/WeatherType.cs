using System.Drawing;

namespace MCLevelEdit.Model.Domain;

public enum Weather
{
    Wind = 4
}

public class WeatherType : EntityType
{
    public WeatherType(Weather weather) : base(TypeId.Weather, ((int)weather), weather.ToString()) { }

    public override ModelType[] ModelTypes
    {
        get
        {
            if (_modelTypes is null)
            {
                _modelTypes = Enum.GetValues(typeof(Weather))
                    .Cast<int>()
                    .Select(x => new ModelType() { Id = x, Name = Enum.GetName(typeof(Weather), x) })
                    .ToArray();
            }

            return _modelTypes;
        }
    }
}
