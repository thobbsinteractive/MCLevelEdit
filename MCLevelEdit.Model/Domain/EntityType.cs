namespace MCLevelEdit.Model.Domain;

public enum TypeId
{
    Scenery = 2,
    Spawn = 3,
    Creature = 5,
    Weather = 7,
    Effect = 10,
    Switch = 11,
    Spell = 12,
    // Magic Carpet 2 only
    Special = 14,
    Vase = 15
}

public class EntityType
{
    private TypeId _typeId;
    private ModelType _model;

    protected ModelType[] _modelTypes;

    /// <summary>The game whose entity table this type belongs to.</summary>
    public GameVersion Game { get; init; } = GameVersion.MagicCarpet1;

    public virtual ModelType[] ModelTypes => EntityCatalog.GetModelTypes(Game, _typeId);

    public TypeId TypeId
    {
        get { return _typeId; }
        set { _typeId = value; }
    }

    public ModelType Model
    {
        get { return _model; }
        set { _model = value; }
    }

    public EntityType(TypeId typeId, int id, string name, uint mana = 0)
    {
        _typeId = typeId;
        _model = new ModelType()
        {
            Id = id,
            Name = name,
            Mana = mana
        };
    }

    public EntityType(TypeId typeId)
    {
        _typeId = typeId;
    }

    public EntityType Copy()
    {
        return new EntityType(_typeId)
        {
            Game = Game,
            Model = _model.Copy()
        };
    }
};
