using System;
using System.Collections.Generic;
using RecipaediaEX.Overlay;
using SCIENEW.RecipaediaEX.Recipes;

namespace SCIENEW.RecipaediaEX.Overlay;

internal sealed class ReactionEquationPlacableRecipe : IPlacableRecipe
{
	private readonly ReactionEquationRecipe m_recipe;

	public ReactionEquationPlacableRecipe(ReactionEquationRecipe recipe)
	{
		m_recipe = recipe;
	}

	public IReadOnlyList<PlacementRequirement> GetPlacementRequirements()
	{
		List<PlacementRequirement> list = new List<PlacementRequirement>();
		for (int i = 0; i < m_recipe.MaterialsItems.Length; i++)
		{
			EquationItem item = m_recipe.MaterialsItems[i];
			list.Add(new PlacementRequirement
			{
				AddressKind = PlacementAddressKind.ContainerSlot,
				AddressIndex = i,
				MatchKey = FormatItemKey(item),
				Quantity = item.Count,
				AcceptedBlockValues = (item.Values ?? Array.Empty<int>())
			});
		}
		for (int j = 0; j < m_recipe.MaterialsFluids.Length; j++)
		{
			EquationFluid equationFluid = m_recipe.MaterialsFluids[j];
			list.Add(new PlacementRequirement
			{
				AddressKind = PlacementAddressKind.ContainerSlot,
				AddressIndex = 100 + j,
				MatchKey = equationFluid.Value.ToString(),
				Quantity = equationFluid.Volume,
				AcceptedBlockValues = Array.Empty<int>()
			});
		}
		return list;
	}

	private static string FormatItemKey(EquationItem item)
	{
		int[] values = item.Values;
		if (values != null && values.Length > 0)
		{
			return item.Values[0].ToString();
		}
		return string.Empty;
	}
}
