﻿using MCLevelEdit.Application.Model;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.ViewModels.Mappers;
using ReactiveUI;
using System.Windows.Input;

namespace MCLevelEdit.ViewModels
{
    public class EntitiesTableViewModel : ViewModelBase
    {
        public ICommand AddNewEntityCommand { get; }
        public ICommand DeleteEntityCommand { get; }

        public EntitiesTableViewModel(EventAggregator<object> eventAggregator, IMapService mapService, ITerrainService terrainService) : base(eventAggregator, mapService, terrainService)
        {
            AddNewEntityCommand = ReactiveCommand.Create(() =>
            {
                AddEntity(new EntityViewModel()
                {
                    Id = Entities.Count,
                    Type = (int)TypeId.Scenary,
                    X = 0,
                    Y = 0,
                    DisId = 0,
                    SwitchSize = 0,
                    SwitchId = 0,
                    Parent = 0,
                    Child = 0
                });
            });

            DeleteEntityCommand = ReactiveCommand.Create<Entity>((entity) =>
            {
                DeleteEntity(entity.ToEntityViewModel());
            });
        }
    }
}
