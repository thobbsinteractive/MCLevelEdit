namespace MCLevelEdit.Model.Domain
{
    public enum Result
    {
        None,
        Pass,
        Warning,
        Fail
    }
    public record ValidationResult(int EntityId, Result Result, string Message);
}
