using Avalonia.Media.Imaging;

namespace MCLevelEdit.Model.Domain;

public class Map
{
    public IList<Entity> Entities { get; set; } = new List<Entity>();
    public Terrain Terrain { get; set; } = new Terrain();
    public WriteableBitmap Preview { get; set; }

    public IList<Entity> GetEntitiesByPosition(Position postion)
    {
        return Entities.Where(e => e.Position == postion).ToList();
    }

    public void SetEntities(IList<Entity> entities)
    {
        Entities.Clear();
        foreach (Entity entity in entities)
            Entities.Add(entity);
    }

    public void AddEntity(Entity entity)
    {
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
