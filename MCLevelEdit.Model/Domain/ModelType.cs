namespace MCLevelEdit.Model.Domain;

public class ModelType
{
    public int Id { get; set; } 
    public string Name { get; set; }
    public ModelType Copy()
    {
        return new ModelType { Id = Id, Name = Name };
    }
}
