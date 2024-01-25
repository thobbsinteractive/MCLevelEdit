using MagicCarpet2Terrain.Model;

namespace MCLevelEdit.ViewModels.Mappers;

public static class TerrainGenerationParamsViewModelToGenerationParams
{
    public static GenerationParameters ToGenerationParameters(this TerrainGenerationParamsViewModel terrainGenerationParamsViewModel)
    {
        return new GenerationParameters()
        {
            MapType = MapType.Day,
            Seed = terrainGenerationParamsViewModel.Seed,
            Offset = terrainGenerationParamsViewModel.Offset,
            Raise = terrainGenerationParamsViewModel.Raise,
            Gnarl = terrainGenerationParamsViewModel.Gnarl,
            River = terrainGenerationParamsViewModel.River,
            LRiver = terrainGenerationParamsViewModel.LRiver,
            Source = terrainGenerationParamsViewModel.Source,
            SnLin = terrainGenerationParamsViewModel.SnLin,
            SnFlt = terrainGenerationParamsViewModel.SnFlt,
            BhLin = terrainGenerationParamsViewModel.BhLin,
            BhFlt = terrainGenerationParamsViewModel.BhFlt,
            RkSte = terrainGenerationParamsViewModel.RkSte
        };
    }
}
