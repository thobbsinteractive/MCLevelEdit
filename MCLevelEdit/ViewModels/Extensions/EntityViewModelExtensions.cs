namespace MCLevelEdit.ViewModels.Extensions
{
    public static class EntityViewModelExtensions
    {
        public static bool EqualsTypeAndModel(this EntityViewModel objectA, EntityViewModel objectB) => objectA.Type == objectB.Type && objectA.Model == objectB.Model;
    }

}
