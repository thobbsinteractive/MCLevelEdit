namespace MCLevelEdit.Model.Domain;

public class Wizard
{
    public string Name { get; set; }
    public bool IsActive { get; set; }
    public byte Agression { get; set; }
    public byte Perception { get; set; }
    public byte Reflexes { get; set; }
    public byte CastleLevel { get; set; }
    public Spells Spells { get; set; } = new Spells();
}
