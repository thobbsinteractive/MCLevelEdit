using MCLevelEdit.Model.Domain;
using MCLevelEdit.Model.Domain.MC2;

namespace MCLevelEdit.ViewModels.Mappers;

public static class WizardViewModelToWizard
{
    public static Wizard ToWizard(this WizardViewModel wizardViewModel)
    {
        // A view model with MC2 spell rows came from an MC2 wizard
        Wizard wizard;
        if (wizardViewModel.Mc2Spells.Count > 0)
        {
            var mc2Wizard = new Mc2Wizard { Life = wizardViewModel.Life };
            mc2Wizard.ApplyMc2Spells(wizardViewModel.Mc2Spells);
            wizard = mc2Wizard;
        }
        else
        {
            wizard = new Wizard();
        }

        wizard.Name = wizardViewModel.Name;
        wizard.IsActive = wizardViewModel.IsActive;
        wizard.Agression = wizardViewModel.Agression;
        wizard.Perception = wizardViewModel.Perception;
        wizard.Reflexes = wizardViewModel.Reflexes;
        wizard.CastleLevel = wizardViewModel.CastleLevel;
        wizard.Spells = wizardViewModel.Spells.ToSpells();
        return wizard;
    }
}
