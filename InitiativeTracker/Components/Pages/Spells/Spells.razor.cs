using InitiativeTracker.Domain.Entities;

namespace InitiativeTracker.Components.Pages.Spells;

public partial class Spells
{
    private int _catalogKey;
    private Spell? _editSpell;
    private SpellPreparationList? PreparationListRef;

    private Task OnEditSelected(Spell? spell) => OnEditSpellChanged(spell);

    private async Task OnEditSpellChanged(Spell? spell)
    {
        _editSpell = spell;
        StateHasChanged();
    }

    private void InvalidateCatalog()
    {
        _catalogKey++;
    }

    private async Task OnAddForPrint(Spell spell)
    {
        PreparationListRef?.AddItem(spell, 1);
        StateHasChanged();
    }
}
