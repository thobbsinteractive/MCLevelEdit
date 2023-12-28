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
    public EntityToolBarViewModel(IMapService mapService, ITerrainService terrainService) : base(mapService, terrainService)
    {
        SpawnEntityViewModel  = new EditEntityViewModel(mapService, terrainService, Model.Domain.TypeId.Spawn);
        SceneriesEntityViewModel = new EditEntityViewModel(mapService, terrainService, Model.Domain.TypeId.Scenary);
        CreaturesEntityViewModel = new EditEntityViewModel(mapService, terrainService, Model.Domain.TypeId.Creature);
        EffectsEntityViewModel = new EditEntityViewModel(mapService, terrainService, Model.Domain.TypeId.Effect);
        SpellsEntityViewModel = new EditEntityViewModel(mapService, terrainService, Model.Domain.TypeId.Spell);
        SwitchesEntityViewModel = new EditEntityViewModel(mapService, terrainService, Model.Domain.TypeId.Switch);
        WeathersEntityViewModel = new EditEntityViewModel(mapService, terrainService, Model.Domain.TypeId.Weather);
    }
}
