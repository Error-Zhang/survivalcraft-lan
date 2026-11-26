using System.Linq;
using RecipaediaEX;
using RecipaediaEX.Overlay;
using SCIENEW.RecipaediaEX.Recipes;

namespace SCIENEW.RecipaediaEX.Overlay;

public sealed class ReactorPlacementTarget : IRecipePlacementTarget
{
	private readonly ComponentReactor m_reactor;

	public ReactorPlacementTarget(ComponentReactor reactor)
	{
		m_reactor = reactor;
	}

	public bool CanAccept(IRecipe recipe)
	{
		return recipe is ReactionEquationRecipe;
	}

	public PlacementResult TryPlaceRecipe(IRecipe recipe, PlacementSources sources, PlacementOptions options, bool execute)
	{
		if (!(recipe is ReactionEquationRecipe reactionEquationRecipe))
		{
			return PlacementResult.None(new _003C_003Ez__ReadOnlySingleElementList<string>("该配方不支持自动摆放"));
		}
		PlacementResult items = ContainerSlotPlacementPlanner.TryPlaceEquationItems(m_reactor, 0, 3, reactionEquationRecipe.MaterialsItems, sources, options, execute);
		PlacementResult fluids = FluidContainerPlacementPlanner.TryPlaceEquationFluids(m_reactor, 0, 3, reactionEquationRecipe.MaterialsFluids, sources, execute);
		PlacementResult result = MergeResults(items, fluids);
		if (execute && (result.Success || result.PartialSuccess))
		{
			m_reactor.FindEquation();
		}
		return result;
	}

	private static PlacementResult MergeResults(PlacementResult items, PlacementResult fluids)
	{
		bool moved = items.HadTransfers || fluids.HadTransfers;
		if (items.Success && fluids.Success)
		{
			return PlacementResult.Complete(moved);
		}
		string[] missing = items.Missing.Concat(fluids.Missing).Distinct().ToArray();
		return moved ? PlacementResult.Partial(missing) : PlacementResult.None(missing);
	}
}
