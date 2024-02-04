using MCLevelEdit.Model.Domain;

namespace MCLevelEdit.ViewModels.Mappers;

public static class WizardToWizardViewModel
{
    public static WizardViewModel ToWizardViewModel(this Wizard wizard)
    {
        return new WizardViewModel()
        {
            Name = wizard.Name,
            IsActive = wizard.IsActive,
            Agression = wizard.Agression,
            Perception = wizard.Perception,
            Reflexes = wizard.Reflexes,
            CastleLevel = wizard.CastleLevel
        };
    }
}
