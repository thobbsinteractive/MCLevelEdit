using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Linq;

namespace MCLevelEdit.DataModel
{
    public class Map : ObservableObject
    {
        private IList<Entity> _entities;
        byte[] _heightMap;
        private WriteableBitmap _preview;
        private TerrainGenerationParameters _terrainGenerationParameters;

        public TerrainGenerationParameters TerrainGenerationParameters
        {
            get { return _terrainGenerationParameters; }
            set { SetProperty(ref _terrainGenerationParameters, value); }
        }
        public IList<Entity> Entities 
        { 
            get { return _entities; }
            set { SetProperty(ref _entities, value); }
        }
        public byte[] HeightMap 
        { 
            get { return _heightMap; } 
            set { SetProperty(ref _heightMap, value); } 
        }
        public WriteableBitmap Preview
        {
            get { return _preview; }
            set { SetProperty(ref _preview, value); }
        }

        public Map()
        {
            _entities = new List<Entity>();
            _heightMap = new byte[Globals.MAX_MAP_SIZE * Globals.MAX_MAP_SIZE];
            TerrainGenerationParameters = new TerrainGenerationParameters();
        }

        public IList<Entity> GetEntitiesByPosition(Position postion)
        {
            return _entities.Where(e => e.Position == postion).ToList();
        }

        public void SetEntities(IList<Entity> entities)
        {
            _entities.Clear();
            foreach (Entity entity in entities)
                _entities.Add(entity);
        }

        public void AddEntity(Entity entity)
        {
            //TODO: Validation needed, this assumes for the moment that more than 1 entity can be on one square
            if (this.Entities.Count < Globals.MAX_ENTITIES)
                this.Entities.Add(entity);
        }

        public void RemoveEntity(Entity entity)
        {
            this.Entities.Remove(entity);

            //Update Indexes
            for(int i = 0; i < this.Entities.Count; i++)
            {
                this.Entities[i].Id = 0;
            }
        }
    }
}
