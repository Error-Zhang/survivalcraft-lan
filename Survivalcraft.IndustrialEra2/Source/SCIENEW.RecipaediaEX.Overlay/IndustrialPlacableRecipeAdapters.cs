using RecipaediaEX;
using RecipaediaEX.Overlay;
using SCIENEW.RecipaediaEX.Recipes;

namespace SCIENEW.RecipaediaEX.Overlay;

public static class IndustrialPlacableRecipeAdapters
{
	public static void Register()
	{
		PlacableRecipeAdapter.Register(TryOne2One);
		PlacableRecipeAdapter.Register(TryWireComposing);
		PlacableRecipeAdapter.Register(TryReactionEquation);
	}

	private static IPlacableRecipe? TryOne2One(IRecipe recipe)
	{
		if (!(recipe is One2OneRecipe recipe2))
		{
			return null;
		}
		return new One2OnePlacableRecipe(recipe2);
	}

	private static IPlacableRecipe? TryWireComposing(IRecipe recipe)
	{
		if (!(recipe is WireComposingRecipe recipe2))
		{
			return null;
		}
		return new WireComposingPlacableRecipe(recipe2);
	}

	private static IPlacableRecipe? TryReactionEquation(IRecipe recipe)
	{
		if (!(recipe is ReactionEquationRecipe recipe2))
		{
			return null;
		}
		return new ReactionEquationPlacableRecipe(recipe2);
	}
}
