using System.Collections.Generic;
using RecipaediaEX.Overlay;
using SCIENEW.RecipaediaEX.Recipes;
using SCIENEW.Utils;

namespace SCIENEW.RecipaediaEX.Overlay;

internal sealed class One2OnePlacableRecipe : IPlacableRecipe
{
	private readonly One2OneRecipe m_recipe;

	public One2OnePlacableRecipe(One2OneRecipe recipe)
	{
		m_recipe = recipe;
	}

	public IReadOnlyList<PlacementRequirement> GetPlacementRequirements()
	{
		return new _003C_003Ez__ReadOnlySingleElementList<PlacementRequirement>(new PlacementRequirement
		{
			AddressKind = PlacementAddressKind.ContainerSlot,
			AddressIndex = 0,
			MatchKey = m_recipe.Ingredient,
			Quantity = m_recipe.IngredientCount,
			AcceptedBlockValues = RecipesUtils.ExpandIngredientToBlockValues(m_recipe.Ingredient)
		});
	}
}
