using InitiativeTracker.Domain.Entities;

namespace InitiativeTracker.Components.Pages.Spells;

public partial class Spells
{
    private int _catalogKey;
    private SpellEntity? _editSpell;
    private SpellPreparationList? PreparationListRef;

    private Task OnEditSelected(SpellEntity? spell) => OnEditSpellChanged(spell);

    private async Task OnEditSpellChanged(SpellEntity? spell)
    {
        _editSpell = spell;
        StateHasChanged();
    }

    private void InvalidateCatalog()
    {
        _catalogKey++;
    }

    private async Task OnAddForPrint(SpellEntity spell)
    {
        PreparationListRef?.AddItem(spell, 1);
        StateHasChanged();
    }
}
