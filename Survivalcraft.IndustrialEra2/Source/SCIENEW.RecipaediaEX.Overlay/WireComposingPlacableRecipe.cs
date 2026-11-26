using System.Collections.Generic;
using RecipaediaEX.Overlay;
using SCIENEW.RecipaediaEX.Recipes;
using SCIENEW.Utils;

namespace SCIENEW.RecipaediaEX.Overlay;

internal sealed class WireComposingPlacableRecipe : IPlacableRecipe
{
	private readonly WireComposingRecipe m_recipe;

	public WireComposingPlacableRecipe(WireComposingRecipe recipe)
	{
		m_recipe = recipe;
	}

	public IReadOnlyList<PlacementRequirement> GetPlacementRequirements()
	{
		return new _003C_003Ez__ReadOnlyArray<PlacementRequirement>(new PlacementRequirement[2]
		{
			new PlacementRequirement
			{
				AddressKind = PlacementAddressKind.ContainerSlot,
				AddressIndex = 0,
				MatchKey = m_recipe.Ingredient,
				Quantity = m_recipe.IngredientCount,
				AcceptedBlockValues = RecipesUtils.ExpandIngredientToBlockValues(m_recipe.Ingredient)
			},
			new PlacementRequirement
			{
				AddressKind = PlacementAddressKind.ContainerSlot,
				AddressIndex = 1,
				MatchKey = m_recipe.WireIngredient,
				Quantity = m_recipe.WireIngredientCount,
				AcceptedBlockValues = RecipesUtils.ExpandIngredientToBlockValues(m_recipe.WireIngredient)
			}
		});
	}
}
