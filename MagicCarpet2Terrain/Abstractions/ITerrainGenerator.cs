using MagicCarpet2Terrain.Model;
using System.Threading.Tasks;

namespace MagicCarpet2Terrain.Abstractions
{
    public interface ITerrainGenerator
    {
        Task<Terrain> CalculateTerrainAsync(GenerationParameters genParams, byte stage = 18);
        Terrain CalculateTerrain(GenerationParameters genParams, byte stage = 18);
    }
}
