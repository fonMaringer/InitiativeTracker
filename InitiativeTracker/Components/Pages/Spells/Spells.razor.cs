using InitiativeTracker.Domain.Entities;

namespace InitiativeTracker.Components.Pages.Spells;

public partial class Spells
{
    private Spell? _editSpell;
    private SpellPreparationList? PreparationListRef;
    private SpellCatalog? Catalog;

    private Task OnEditSelected(Spell? spell) => OnEditSpellChanged(spell);

    private async Task OnEditSpellChanged(Spell? spell)
    {
        _editSpell = spell;
        StateHasChanged();
    }

    private async Task InvalidateCatalog()
    {
        if (Catalog is not null)
            await Catalog.OnSearch();
    }

    private async Task OnAddForPrint(Spell spell)
    {
        PreparationListRef?.AddItem(spell, 1);
        StateHasChanged();
    }
}
