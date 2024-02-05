using MCLevelEdit.Model.Domain;

namespace MCLevelEdit.ViewModels.Mappers;

public static class WizardViewModelToWizard
{
    public static Wizard ToWizard(this WizardViewModel wizardViewModel)
    {
        return new Wizard()
        {
            Name = wizardViewModel.Name,
            IsActive = wizardViewModel.IsActive,
            Agression = wizardViewModel.Agression,
            Perception = wizardViewModel.Perception,
            Reflexes = wizardViewModel.Reflexes,
            CastleLevel = wizardViewModel.CastleLevel,
            Spells = wizardViewModel.Spells.ToSpells()
        };
    }
}
