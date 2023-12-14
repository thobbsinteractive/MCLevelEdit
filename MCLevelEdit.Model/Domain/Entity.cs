namespace MCLevelEdit.Model.Domain;

public class Entity
{
    public int Id { get; set; }
    public Position Position { get; set; }
    public EntityType EntityType { get; set; }
    public ushort Parent { get; set; }
    public ushort Child { get; set; }

    public Entity Copy()
    {
        return new Entity()
        {
            Id = this.Id,
            Position = this.Position.Copy(),
            EntityType = this.EntityType.Copy(),
            Parent = this.Parent,
            Child = this.Child
        };
    }
};
