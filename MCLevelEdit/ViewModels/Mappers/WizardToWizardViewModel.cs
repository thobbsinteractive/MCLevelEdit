using MCLevelEdit.Model.Domain;
using MCLevelEdit.Model.Domain.MC2;

namespace MCLevelEdit.ViewModels.Mappers;

public static class WizardToWizardViewModel
{
    /// <param name="hiddenWorlds">The wizard is in a Hidden Worlds level, which names one spell differently.</param>
    public static WizardViewModel ToWizardViewModel(this Wizard wizard, bool hiddenWorlds = false)
    {
        return new WizardViewModel()
        {
            Name = wizard.Name,
            IsActive = wizard.IsActive,
            Agression = wizard.Agression,
            Perception = wizard.Perception,
            Reflexes = wizard.Reflexes,
            CastleLevel = wizard.CastleLevel,
            Spells = wizard.Spells.ToSpellsViewModel(hiddenWorlds),
            Life = (wizard as Mc2Wizard)?.Life ?? 0,
            Mc2Spells = (wizard as Mc2Wizard)?.ToMc2SpellViewModels() ?? new()
        };
    }
}
