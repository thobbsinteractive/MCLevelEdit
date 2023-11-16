using DynamicData;
using MCLevelEdit.DataModel;
using MCLevelEdit.Interfaces;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;

namespace MCLevelEdit.ViewModels
{
    public class EntitiesViewModel : ViewModelBase
    {
        public ICommand AddNewEntityCommand { get; }
        public ICommand DeleteEntityCommand { get; }

        public static KeyValuePair<int, string>[] TypeIds { get; } =
            Enum.GetValues(typeof(TypeId))
            .Cast<int>()
            .Select(x => new KeyValuePair<int, string>(key: x, value: Enum.GetName(typeof(TypeId), x)))
            .ToArray();

        public EntitiesViewModel(IMapService mapService, ITerrainService terrainService) : base(mapService, terrainService)
        {
            AddNewEntityCommand = ReactiveCommand.Create(() =>
            {
                AddEntity(new ScenaryType(Scenary.Tree), new Position(0,0));
            });

            DeleteEntityCommand = ReactiveCommand.Create<Entity>((entity) =>
            {
                DeleteEntity(entity);
            });
        }
    }
}
