using System.Collections.Generic;
using RecipaediaEX;
using RecipaediaEX.Overlay;
using SCIENEW.RecipaediaEX.Recipes;

namespace SCIENEW.RecipaediaEX.Overlay;

public sealed class WireComposerPlacementTarget : IRecipePlacementTarget
{
	private readonly ComponentWireComposer m_composer;

	public WireComposerPlacementTarget(ComponentWireComposer composer)
	{
		m_composer = composer;
	}

	public bool CanAccept(IRecipe recipe)
	{
		return recipe is WireComposingRecipe;
	}

	public PlacementResult TryPlaceRecipe(IRecipe recipe, PlacementSources sources, PlacementOptions options, bool execute)
	{
		if (!(recipe is WireComposingRecipe wireComposingRecipe))
		{
			return PlacementResult.None(new _003C_003Ez__ReadOnlySingleElementList<string>("该配方不支持自动摆放"));
		}
		PlacementResult first = ContainerSlotPlacementPlanner.TryPlaceSingleIngredientSlot(m_composer, 0, wireComposingRecipe.Ingredient, wireComposingRecipe.IngredientCount, sources, options, execute);
		PlacementResult second = ContainerSlotPlacementPlanner.TryPlaceSingleIngredientSlot(m_composer, m_composer.IngredientSlotIndex, wireComposingRecipe.WireIngredient, wireComposingRecipe.WireIngredientCount, sources, options, execute);
		PlacementResult result = MergeResults(first, second);
		if (execute && (result.Success || result.PartialSuccess))
		{
			m_composer.FindRecipe();
		}
		return result;
	}

	private static PlacementResult MergeResults(PlacementResult first, PlacementResult second)
	{
		if (first.Success && second.Success)
		{
			return PlacementResult.Complete(first.HadTransfers || second.HadTransfers);
		}
		if (first.Success && !second.Success && second.PartialSuccess)
		{
			return PlacementResult.Partial(second.Missing, first.HadTransfers || second.HadTransfers);
		}
		if (second.Success && !first.Success && first.PartialSuccess)
		{
			return PlacementResult.Partial(first.Missing, first.HadTransfers || second.HadTransfers);
		}
		if (!first.Success && !second.Success && (first.PartialSuccess || second.PartialSuccess))
		{
			List<string> list = new List<string>();
			foreach (string item in first.Missing)
			{
				if (!list.Contains(item))
				{
					list.Add(item);
				}
			}
			foreach (string item2 in second.Missing)
			{
				if (!list.Contains(item2))
				{
					list.Add(item2);
				}
			}
			return PlacementResult.Partial(list, first.HadTransfers || second.HadTransfers);
		}
		if (!first.Success && first.Missing.Count > 0)
		{
			return first;
		}
		if (!second.Success && second.Missing.Count > 0)
		{
			return second;
		}
		if (!first.Success)
		{
			return first;
		}
		return second;
	}
}
