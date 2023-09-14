﻿using Avalonia.Data.Converters;
using MCLevelEdit.DataModel;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;

namespace MCLevelEdit.ViewModels
{
    public class TypeIdConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var typeId = (TypeId)value;
            return new KeyValuePair<int, string>(key: (int)typeId, value: Enum.GetName(typeof(TypeId), typeId));
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return (TypeId)((KeyValuePair<int, string>)value).Key;
        }
    }

    public class EntitiesViewModel : ViewModelBase
    {
        public ObservableCollection<Entity> Entities { get; }
        
        public static KeyValuePair<int, string>[] TypeIds { get; } =
            Enum.GetValues(typeof(TypeId))
            .Cast<int>()
            .Select(x => new KeyValuePair<int, string>(key: x, value: Enum.GetName(typeof(TypeId), x)))
            .ToArray();

        public ICommand AddNewEntityCommand { get; }

        public EntitiesViewModel()
        {
            Entities = new ObservableCollection<Entity>
            {
                new Entity(0, DataModel.EntityTypes.I.Spawns[(int)Spawn.Flyer1], new Position(0, 0)),
                new Entity(1, DataModel.EntityTypes.I.Creatures[(int)Creature.Archer], new Position(0, 1))
            };

            AddNewEntityCommand = ReactiveCommand.Create(() =>
            {
                int i = 0;
                // Code here will be executed when the button is clicked.
            });
        }
    }
}
