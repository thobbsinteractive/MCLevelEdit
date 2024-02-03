using MCLevelEdit.Model.Domain;

namespace MCLevelEdit.ViewModels.Mappers;

public static class ValidationResultToValidationResultViewModel
{
    public static ValidationResultViewModel ToValidationResultViewModel(this ValidationResult validationResult)
    {
        return new ValidationResultViewModel(validationResult.EntityId, validationResult.Result.ToString(), validationResult.Message);
    }
}
