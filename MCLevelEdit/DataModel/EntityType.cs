using Avalonia.Data.Converters;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Globalization;
using System;
using Avalonia.Media;

namespace MCLevelEdit.DataModel
{
    public enum TypeId
    {
        Scenary = 2,
        Spawn = 3,
        Creature = 5,
        Weather = 7,
        Effect = 10,
        Switch = 11,
        Spell = 12
    }

    public class TypeIdConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not null)
            {
                var entityType = (EntityType)value;
                return new KeyValuePair<int, string>(key: (int)entityType.TypeId, value: Enum.GetName(typeof(TypeId), entityType.TypeId));
            }
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return EntityTypeExtensions.GetEntityFromTypeId((TypeId)((KeyValuePair<int, string>)value).Key);
        }
    }

    public class EntityTypeToNameConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var entityType = (KeyValuePair<int, string>)value;
                return entityType.Value;
            }
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EntityType : ObservableObject
    {
        private TypeId _typeId;
        private ModelType _model;
        private Color _colour;

        protected ModelType[] _modelTypes;

        public virtual ModelType[] ModelTypes
        {
            get
            {
                return new ModelType[] { };
            }
        }

        public TypeId TypeId
        {
            get { return _typeId; }
            set { SetProperty(ref _typeId, value); }
        }

        public ModelType Model
        {
            get { return _model; }
            set { 
                SetProperty(ref _model, value); 
            }
        }

        public Color Colour
        {
            get { return _colour; }
        }

        public EntityType(TypeId typeId, Color colour, int id, string name)
        {
            _typeId = typeId;
            _colour = colour;
            _model = new ModelType()
            {
                Id = id,
                Name = name
            };
        }

        public EntityType(TypeId typeId, Color colour)
        {
            _typeId = typeId;
            _colour = colour;
        }

        public EntityType Copy()
        {
            return new EntityType(_typeId, _colour)
            {
                Model = _model.Copy()
            };
        }
    };

    public static class EntityTypeExtensions
    {
        public static EntityType GetEntityFromTypeId(this TypeId typeId)
        {
            switch (typeId)
            {
                case TypeId.Scenary:
                    return new ScenaryType(Scenary.Tree);
                case TypeId.Spawn:
                    return new SpawnType(Spawn.Flyer1);
                case TypeId.Creature:
                    return new CreatureType(Creature.Vulture);
                case TypeId.Effect:
                    return new EffectType(Effect.Unknown0);
                case TypeId.Weather:
                    return new WeatherType(Weather.Wind);
                case TypeId.Spell:
                    return new SpellType(Spell.Fireball);
                case TypeId.Switch:
                    return new SwitchType(Switch.HiddenInside);
                default:
                    return new EntityType(typeId, Color.FromRgb(0, 0, 0), 0, "");
            }
        }

        public static EntityType GetEntityFromTypeIdAndChildId(this TypeId typeId, int childId)
        {
            switch (typeId)
            {
                case TypeId.Scenary:
                    return EntityTypes.I.Sceneries[childId];
                case TypeId.Spawn:
                    return EntityTypes.I.Spawns[childId];
                case TypeId.Creature:
                    return EntityTypes.I.Creatures[childId];
                case TypeId.Weather:
                    return EntityTypes.I.Weathers[childId];
                case TypeId.Effect:
                    return EntityTypes.I.Effects[childId];
                case TypeId.Switch:
                    return EntityTypes.I.Switches[childId];
                case TypeId.Spell:
                    return EntityTypes.I.Spells[childId];
            }
            return null;
        }
    }
}
