using MCLevelEdit.Model.Domain.MC1; // MC1 effect IDs for paths, walls, canyons etc. are the same in MC2
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

    public virtual Entity Copy() => CopyTo(new Entity());

    /// <summary>Copies the shared fields onto <paramref name="target"/>; subclasses add their own.</summary>
    protected T CopyTo<T>(T target) where T : Entity
    {
        target.Id = this.Id;
        target.Position = this.Position.Copy();
        target.EntityType = this.EntityType.Copy();
        target.DisId = this.DisId;
        target.SwitchSize = this.SwitchSize;
        target.SwitchId = this.SwitchId;
        target.Parent = this.Parent;
        target.Child = this.Child;
        return target;
    }

    /// <summary>
    /// Type 0 slots that still hold data. The game ignores them: remc2's GenerateEvents_49290 sets an
    /// entity's type to 0 once its terrain feature has been built. They are kept in the file but hidden.
    /// </summary>
    public bool IsInert() => (int)this.EntityType.TypeId == 0;

    public bool IsBuilding() => this?.EntityType.TypeId == TypeId.Effect && this?.EntityType.Model.Id == (int)Effect.VillagerBuilding;
    public bool IsPath() => this?.EntityType.TypeId == TypeId.Effect && this?.EntityType.Model.Id == (int)Effect.Path;
    public bool IsWall() => this?.EntityType.TypeId == TypeId.Effect && this?.EntityType.Model.Id == (int)Effect.Wall;
    public bool IsCanyon() => this?.EntityType.TypeId == TypeId.Effect && this?.EntityType.Model.Id == (int)Effect.Canyon;
    public bool IsRidge() => this?.EntityType.TypeId == TypeId.Effect && this?.EntityType.Model.Id == (int)Effect.RidgeNode;
    public bool IsPathOrWall() => IsPath() || IsWall();
    public bool IsCanyonOrRidge() => IsCanyon() || IsRidge();
    public bool IsPathEntity() => IsPathOrWall() || IsCanyonOrRidge();
    public bool IsSwitch() => this?.EntityType.TypeId == TypeId.Switch;
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
