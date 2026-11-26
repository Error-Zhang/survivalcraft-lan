using System;
using System.Collections.Generic;
using Game;
using RecipaediaEX;
using RecipaediaEX.Overlay;
using SCIENEW.ProductionIO;
using SCIENEW.RecipaediaEX.Recipes;

namespace SCIENEW.RecipaediaEX.Overlay;

public sealed class One2OnePlacementTarget : IRecipePlacementTarget
{
	private readonly IInventory m_container;

	private readonly int m_inputSlot;

	private readonly Action m_refreshRecipe;

	public One2OnePlacementTarget(IInventory container, int inputSlot, Action refreshRecipe)
	{
		m_container = container;
		m_inputSlot = inputSlot;
		m_refreshRecipe = refreshRecipe;
	}

	public bool CanAccept(IRecipe recipe)
	{
		return recipe is One2OneRecipe;
	}

	public PlacementResult TryPlaceRecipe(IRecipe recipe, PlacementSources sources, PlacementOptions options, bool execute)
	{
		if (!(recipe is One2OneRecipe one2OneRecipe))
		{
			return PlacementResult.None(new _003C_003Ez__ReadOnlySingleElementList<string>("该配方不支持自动摆放"));
		}
		int targetSlot = m_inputSlot;
		if (m_container is IInventoryProductionSlots inventoryProductionSlots)
		{
			using IEnumerator<int> enumerator = inventoryProductionSlots.GetSlotIndices(ProductionSlotKind.Input).GetEnumerator();
			if (enumerator.MoveNext())
			{
				targetSlot = enumerator.Current;
			}
		}
		PlacementResult result = ContainerSlotPlacementPlanner.TryPlaceSingleIngredientSlot(m_container, targetSlot, one2OneRecipe.Ingredient, one2OneRecipe.IngredientCount, sources, options, execute);
		if (execute && (result.Success || result.PartialSuccess))
		{
			m_refreshRecipe();
		}
		return result;
	}
}
