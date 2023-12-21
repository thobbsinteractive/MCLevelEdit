﻿using MCLevelEdit.Model.Domain;

namespace MCLevelEdit.ViewModels.Mappers;

public static class GenerationParamsToTerrainGenerationParamsViewModel
{
    public static TerrainGenerationParamsViewModel ToTerrainGenerationParamsViewModel(this GenerationParameters terrainGenerationParamsViewModel)
    {
        return new TerrainGenerationParamsViewModel()
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