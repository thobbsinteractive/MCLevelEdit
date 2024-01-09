﻿using MCLevelEdit.Application.Model;
using MCLevelEdit.Model.Abstractions;

namespace MCLevelEdit.ViewModels;

public class EntityToolBarViewModel : ViewModelBase
{
    public EditEntityViewModel SpawnEntityViewModel { get; init; }
    public EditEntityViewModel SceneriesEntityViewModel { get; init; }
    public EditEntityViewModel CreaturesEntityViewModel { get; init; }
    public EditEntityViewModel EffectsEntityViewModel { get; init; }
    public EditEntityViewModel SpellsEntityViewModel { get; init; }
    public EditEntityViewModel SwitchesEntityViewModel { get; init; }
    public EditEntityViewModel WeathersEntityViewModel { get; init; }
    public EntityToolBarViewModel(EventAggregator<object> eventAggregator, IMapService mapService, ITerrainService terrainService) : base(eventAggregator, mapService, terrainService)
    {
        SpawnEntityViewModel  = new EditEntityViewModel(eventAggregator, mapService, terrainService, Model.Domain.TypeId.Spawn);
        SceneriesEntityViewModel = new EditEntityViewModel(eventAggregator, mapService, terrainService, Model.Domain.TypeId.Scenary);
        CreaturesEntityViewModel = new EditEntityViewModel(eventAggregator, mapService, terrainService, Model.Domain.TypeId.Creature);
        EffectsEntityViewModel = new EditEntityViewModel(eventAggregator, mapService, terrainService, Model.Domain.TypeId.Effect);
        SpellsEntityViewModel = new EditEntityViewModel(eventAggregator, mapService, terrainService, Model.Domain.TypeId.Spell);
        SwitchesEntityViewModel = new EditEntityViewModel(eventAggregator, mapService, terrainService, Model.Domain.TypeId.Switch);
        WeathersEntityViewModel = new EditEntityViewModel(eventAggregator, mapService, terrainService, Model.Domain.TypeId.Weather);
    }
}
