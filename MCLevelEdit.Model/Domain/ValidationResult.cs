using MCLevelEdit.Model.Enums;

namespace MCLevelEdit.Model.Domain;

public record ValidationResult(int EntityId, Result Result, string Message);
