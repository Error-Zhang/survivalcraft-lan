using Engine;
using GameEntitySystem;

namespace Game;

/// <summary>
/// Single-player UI compatibility for the optional Neorxna mod.
/// </summary>
public static class NeorxnaCompatRuntime {
    private const string RecipeDialogTypeName = "Neorxna.NUI.NUIRecipaediaRecipesDialog";

    public static void FixRecipeDialogParent(ref ContainerWidget parentWidget, Dialog dialog) {
        if (!string.Equals(
                dialog?.GetType().FullName,
                RecipeDialogTypeName,
                StringComparison.Ordinal)) {
            return;
        }

        ComponentPlayer mainPlayer = GameManager.Project?
            .FindSubsystem<SubsystemPlayers>()?
            .MainPlayer;
        if (mainPlayer?.GuiWidget != null) {
            parentWidget = mainPlayer.GuiWidget;
        }
    }
}
