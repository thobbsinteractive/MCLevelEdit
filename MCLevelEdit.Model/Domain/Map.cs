namespace MCLevelEdit.Model.Domain;

public class Map
{
    public IList<Entity> Entities { get; set; } = new List<Entity>();
    public Terrain Terrain { get; set; } = new Terrain();

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

    public void DeleteEntity(Entity entity)
    {
        int index = GetIndexOf(entity);
        if (index > -1)
            this.Entities.RemoveAt(index);
    }

    public void UpdateEntity(Entity entity)
    {
        int index = GetIndexOf(entity);
        if (index > -1)
            this.Entities[index] = entity;
    }

    private int GetIndexOf(Entity entity)
    {
        for (int i = 0; i < this.Entities.Count; i++)
        {
            if(this.Entities[i].Id == entity.Id)
            {
                return i;
            }
        }
        return -1;
    }
}
