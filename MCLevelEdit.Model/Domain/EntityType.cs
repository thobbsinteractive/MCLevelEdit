﻿using Avalonia.Media;

namespace MCLevelEdit.Model.Domain;

public enum TypeId
{
    Scenary = 2,
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
    private Color _colour;

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

    public Color Colour
    {
        get { return _colour; }
    }

    public EntityType(TypeId typeId, Color colour, int id, string name)
    {
        _typeId = typeId;
        _colour = colour;
        _model = new ModelType()
        {
            Id = id,
            Name = name
        };
    }

    public EntityType(TypeId typeId, Color colour)
    {
        _typeId = typeId;
        _colour = colour;
    }

    public EntityType Copy()
    {
        return new EntityType(_typeId, _colour)
        {
            Model = _model.Copy()
        };
    }
};
