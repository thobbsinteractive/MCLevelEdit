using MCLevelEdit.Model.Abstractions;

namespace MCLevelEdit.ViewModels;

public class EntityToolBarViewModel : ViewModelBase
{
    public CreateEntityViewModel SpawnEntityViewModel { get; init; }
    public CreateEntityViewModel SceneriesEntityViewModel { get; init; }
    public CreateEntityViewModel CreaturesEntityViewModel { get; init; }
    public CreateEntityViewModel EffectsEntityViewModel { get; init; }
    public CreateEntityViewModel SpellsEntityViewModel { get; init; }
    public CreateEntityViewModel SwitchesEntityViewModel { get; init; }
    public CreateEntityViewModel WeathersEntityViewModel { get; init; }
    public EntityToolBarViewModel(IMapService mapService, ITerrainService terrainService) : base(mapService, terrainService)
    {
        SpawnEntityViewModel  = new CreateEntityViewModel(mapService, terrainService, Model.Domain.TypeId.Spawn);
        SceneriesEntityViewModel = new CreateEntityViewModel(mapService, terrainService, Model.Domain.TypeId.Scenary);
        CreaturesEntityViewModel = new CreateEntityViewModel(mapService, terrainService, Model.Domain.TypeId.Creature);
        EffectsEntityViewModel = new CreateEntityViewModel(mapService, terrainService, Model.Domain.TypeId.Effect);
        SpellsEntityViewModel = new CreateEntityViewModel(mapService, terrainService, Model.Domain.TypeId.Spell);
        SwitchesEntityViewModel = new CreateEntityViewModel(mapService, terrainService, Model.Domain.TypeId.Switch);
        WeathersEntityViewModel = new CreateEntityViewModel(mapService, terrainService, Model.Domain.TypeId.Weather);
    }
}
