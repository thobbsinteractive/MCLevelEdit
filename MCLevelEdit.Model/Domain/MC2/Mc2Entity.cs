namespace MCLevelEdit.Model.Domain.MC2;

/// <summary>A Magic Carpet 2 entity: an <see cref="Entity"/> plus the extra field MC2 entity slots have.</summary>
public class Mc2Entity : Entity
{
    /// <summary>The last field of an MC2 entity slot (par3_18 in remc2).</summary>
    public ushort Par3 { get; set; }

    public override Entity Copy()
    {
        var copy = CopyTo(new Mc2Entity());
        copy.Par3 = this.Par3;
        return copy;
    }
}
