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

    public bool IsBuilding() => this?.EntityType.TypeId == TypeId.Effect && this?.EntityType.Model.Id == (int)Effect.VillagerBuilding;
    public bool IsPath() => this?.EntityType.TypeId == TypeId.Effect && this?.EntityType.Model.Id == (int)Effect.Path;
    public bool IsWall() => this?.EntityType.TypeId == TypeId.Effect && this?.EntityType.Model.Id == (int)Effect.Wall;
    public bool IsCanyon() => this?.EntityType.TypeId == TypeId.Effect && this?.EntityType.Model.Id == (int)Effect.Canyon;
    public bool IsRidge() => this?.EntityType.TypeId == TypeId.Effect && this?.EntityType.Model.Id == (int)Effect.RidgeNode;
    public bool IsPathOrWall() => IsPath() || IsWall();
    public bool IsCanyonOrRidge() => IsCanyon() || IsRidge();
    public bool IsPathEntity() => IsPathOrWall() || IsCanyonOrRidge();
    public bool IsSwitch() => 
        this?.EntityType.TypeId == TypeId.Switch && 
        (this?.EntityType.Model.Id == (int)Switch.DeathInside || 
         this?.EntityType.Model.Id == (int)Switch.DeathOutside ||
         this?.EntityType.Model.Id == (int)Switch.HiddenInside ||
         this?.EntityType.Model.Id == (int)Switch.HiddenOutside ||
         this?.EntityType.Model.Id == (int)Switch.HiddenInsideRe ||
         this?.EntityType.Model.Id == (int)Switch.HiddenOutsideRe);
    public bool IsTeleport() => this?.EntityType.TypeId == TypeId.Effect && this?.EntityType.Model.Id == (int)Effect.Teleport;
    public bool IsSpawn() => this?.EntityType.TypeId == TypeId.Spawn;
    public bool IsFireballSpell() => this?.EntityType.TypeId == TypeId.Spell && this?.EntityType.Model.Id == (int)Spell.Fireball;
    public bool IsPossessionSpell() => this?.EntityType.TypeId == TypeId.Spell && this?.EntityType.Model.Id == (int)Spell.Possession;
    public bool IsCastleSpell() => this?.EntityType.TypeId == TypeId.Spell && this?.EntityType.Model.Id == (int)Spell.Castle;

    public override string ToString()
    {
        return $"Entity {this.Id}: {this.EntityType.TypeId} - {this.EntityType.Model.Name}:";
    }
};
