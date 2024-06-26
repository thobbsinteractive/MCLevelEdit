﻿namespace MCLevelEdit.Model.Domain;

public enum TypeId
{
    Scenery = 2,
    Spawn = 3,
    Creature = 5,
    Weather = 7,
    Effect = 10,
    Switch = 11,
    Spell = 12
}

public class EntityType
{
    private TypeId _typeId;
    private ModelType _model;

    protected ModelType[] _modelTypes;

    public virtual ModelType[] ModelTypes
    {
        get
        {
            return new ModelType[] { };
        }
    }

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
            Model = _model.Copy()
        };
    }
};
