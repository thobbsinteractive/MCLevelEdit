namespace MCLevelEdit.Model.Domain;

public class Entity
{
    public int Id { get; set; }
    public Position Position { get; set; }
    public EntityType EntityType { get; set; }

    public ushort DisId { get; set; }
    public ushort SwitchSize { get; set; }
    public ushort SwitchId { get; set; }
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

    public bool IsPathOrWall() => this?.EntityType.TypeId == TypeId.Effect && (this?.EntityType.Model.Id == (int)Effect.Wall || this?.EntityType.Model.Id == (int)Effect.Path);
    public bool IsSwitch() => 
        this?.EntityType.TypeId == TypeId.Switch && 
        (this?.EntityType.Model.Id == (int)Switch.DeathInside || 
         this?.EntityType.Model.Id == (int)Switch.DeathOutside ||
         this?.EntityType.Model.Id == (int)Switch.HiddenInside ||
         this?.EntityType.Model.Id == (int)Switch.HiddenOutside ||
         this?.EntityType.Model.Id == (int)Switch.HiddenInsideRe ||
         this?.EntityType.Model.Id == (int)Switch.HiddenOutsideRe);

    public override string ToString()
    {
        return $"Entity {this.Id}: {this.EntityType.TypeId} - {this.EntityType.Model.Name}:";
    }
};
