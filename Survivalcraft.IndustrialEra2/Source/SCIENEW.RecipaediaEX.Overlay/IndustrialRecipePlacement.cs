using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Engine;
using Game;
using Game.Network;
using GameEntitySystem;
using HarmonyLib;
using RecipaediaEX;
using RecipaediaEX.Overlay;
using RecipaediaEX.Search;
using SCIENEW.RecipaediaEX.Recipes;

namespace SCIENEW.RecipaediaEX.Overlay;

internal sealed class IndustrialRecipePlacement(Component component, IRecipePlacementTarget target) : IRecipePlacementTarget {
    private static readonly IReadOnlyList<string> s_waiting = Array.AsReadOnly(Array.Empty<string>());
    private static bool s_installed;

    public static void InstallFeedbackHook() {
        if (s_installed) {
            return;
        }
        Harmony harmony = new("com.IndustrialEra2.LanRecipePlacement");
        harmony.Patch(
            AccessTools.Method(typeof(RecipaediaCraftingOverlayDialog), "ShowPlacementFeedback"),
            prefix: new HarmonyMethod(AccessTools.Method(typeof(IndustrialRecipePlacement), nameof(ShowImmediateFeedback))));
        harmony.Patch(
            AccessTools.Method(typeof(CraftingTablePlacementTarget), nameof(CraftingTablePlacementTarget.TryPlaceRecipe)),
            prefix: new HarmonyMethod(AccessTools.Method(typeof(IndustrialRecipePlacement), nameof(PlaceCraftingRecipe))));
        s_installed = true;
    }

    private static bool ShowImmediateFeedback(PlacementResult result) => !ReferenceEquals(result.Missing, s_waiting);

    private static bool PlaceCraftingRecipe(ComponentCraftingTable ___m_table, IRecipe recipe, PlacementSources sources,
        PlacementOptions options, bool execute, ref PlacementResult __result) {
        if (!execute || !IndustrialEraLanAdapter.IsRemote(___m_table.Project)) {
            return true;
        }
        __result = QueuePlacement(___m_table, recipe, sources, options);
        return false;
    }

    public bool CanAccept(IRecipe recipe) => target.CanAccept(recipe);

    public PlacementResult TryPlaceRecipe(IRecipe recipe, PlacementSources sources, PlacementOptions options, bool execute) {
        if (!execute || !IndustrialEraLanAdapter.IsRemote(component.Project)) {
            return target.TryPlaceRecipe(recipe, sources, options, execute);
        }

        return QueuePlacement(component, recipe, sources, options);
    }

    private static PlacementResult QueuePlacement(Component component, IRecipe recipe, PlacementSources sources, PlacementOptions options) {
        ComponentPlayer player = (sources.PlayerInventory as Component)?.Entity.FindComponent<ComponentPlayer>();
        if (player == null || !ReferenceEquals(player.ComponentMiner.Inventory, sources.PlayerInventory)) {
            return PlacementResult.None([]);
        }
        return IndustrialEraLanAdapter.QueueRecipePlacement(player, component, recipe, sources.ContainerInventory as Component, options)
            ? new PlacementResult { Missing = s_waiting }
            : PlacementResult.None([]);
    }

    internal static IRecipePlacementTarget ResolveTarget(Component component) => component switch {
        ComponentAdvancedCrusher crusher => new One2OnePlacementTarget(crusher, 0, crusher.FindRecipe),
        ComponentKibbler kibbler => new One2OnePlacementTarget(kibbler, 0, kibbler.FindRecipe),
        ComponentMagnetizer magnetizer => new One2OnePlacementTarget(magnetizer, 0, magnetizer.FindRecipe),
        ComponentMercuryPresser presser => new One2OnePlacementTarget(presser, 0, presser.FindRecipe),
        ComponentPresser presser => new One2OnePlacementTarget(presser, 0, presser.FindRecipe),
        ComponentRifling rifling => new One2OnePlacementTarget(rifling, 0, rifling.FindRecipe),
        ComponentSpinner spinner => new One2OnePlacementTarget(spinner, 0, spinner.FindRecipe),
        ComponentSqueezer squeezer => new One2OnePlacementTarget(squeezer, 0, squeezer.FindRecipe),
        ComponentReactor reactor => new ReactorPlacementTarget(reactor),
        ComponentWireComposer composer => new WireComposerPlacementTarget(composer),
        ComponentIEFurnace furnace => new FurnacePlacementTarget(furnace),
        ComponentCraftingTable table => new CraftingTablePlacementTarget(table),
        _ => null
    };

    internal static byte[] Fingerprint(IRecipe recipe) {
        string suffix = recipe switch {
            One2OneRecipe single => FormattableString.Invariant(
                $"/{single.ProcessType}/{single.Ingredient}/{single.IngredientCount}/{single.ResultID}/{single.ResultCount}"),
            WireComposingRecipe wire => FormattableString.Invariant(
                $"/{wire.Ingredient}/{wire.IngredientCount}/{wire.WireIngredient}/{wire.WireIngredientCount}/{wire.ResultID}/{wire.ResultCount}"),
            ReactionEquationRecipe reaction => $"{ItemKey(reaction.MaterialsItems)}/{FluidKey(reaction.MaterialsFluids)}"
                + $"/{ItemKey(reaction.ResultsItems)}/{FluidKey(reaction.ResultsFluids)}",
            _ => string.Empty
        };
        return SHA256.HashData(Encoding.UTF8.GetBytes(RecipaediaRecipeBookmarks.GetRecipeKey(recipe) + suffix));
    }

    private static string ItemKey(EquationItem[] items) =>
        string.Join(";", items.Select(item => item.Count.ToString(CultureInfo.InvariantCulture) + "@" + string.Join(",", item.Values)));

    private static string FluidKey(EquationFluid[] fluids) =>
        string.Join(";", fluids.Select(fluid => FormattableString.Invariant($"{fluid.Value}@{fluid.Volume:R}")));

    internal static void ApplyFeedback(ComponentPlayer player, byte outcome, string missing) {
        int text = outcome switch { 1 => 7, 2 => 9, _ => 8 };
        string message = outcome == 0 && !string.IsNullOrEmpty(missing)
            ? missing : LanguageControl.GetContentWidgets("RecipaediaCraftingOverlay", text);
        player?.ComponentGui.DisplaySmallMessage(message,
            outcome == 0 ? new Color(255, 220, 120, 255) : Color.White, blinking: false, playNotificationSound: false);
    }
}
