using System;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using RecipaediaEX.ComponentsExtra;
using RecipaediaEX.Implementation;
using SCIENEW.RecipaediaEX.Recipes;
using SCIENEW.Utils;

namespace SCIENEW.RecipaediaEX;

public static class DynamicRecipeUtils
{
	public static IRecipe? FromCraftingRecipe(CraftingRecipe? craftingRecipe)
	{
		if (craftingRecipe == null)
		{
			return null;
		}
		if (craftingRecipe.ResultValue == 0 && !string.IsNullOrEmpty(craftingRecipe.Message))
		{
			if (!(craftingRecipe.RequiredHeatLevel > 0f))
			{
				return craftingRecipe.ToFormattedRecipe<OriginalCraftingRecipe>();
			}
			return craftingRecipe.ToFormattedRecipe<OriginalSmeltingRecipe>();
		}
		FormattedRecipe obj = ((craftingRecipe.RequiredHeatLevel > 0f) ? ((FormattedRecipe)craftingRecipe.ToFormattedRecipe<IESmeltingRecipe>()) : ((FormattedRecipe)craftingRecipe.ToFormattedRecipe<IECraftingRecipe>()));
		RecipeMatchExtras.ApplyRecipaediaMatchExtras(obj);
		return obj;
	}

	public static string[]? GetCraftingIngredientStrings(FormattedRecipe formattedRecipe, float requiredHeatLevel)
	{
		if (formattedRecipe.RequiredHeatLevel != requiredHeatLevel)
		{
			return null;
		}
		return GetCraftingIngredientStrings(formattedRecipe);
	}

	public static string[] GetCraftingIngredientStrings(FormattedRecipe formattedRecipe)
	{
		int?[] extraValue = formattedRecipe.GetExtraValue("ActualIngredients", Array.Empty<int?>());
		if (extraValue.Length != 0)
		{
			string[] array = new string[36];
			int num = Math.Min(extraValue.Length, 36);
			for (int i = 0; i < num; i++)
			{
				array[i] = (extraValue[i].HasValue ? RecipesUtils.BlockToCratingID(extraValue[i].Value) : null);
			}
			return array;
		}
		return (string[])formattedRecipe.Ingredients.Clone();
	}

	public static bool TryResolveCraftingContext(IRecipe actual, Project project, out SubsystemTerrain subsystemTerrain, out FormattedRecipe formattedRecipe, out string[] ingredients, out float heatLevel, out float playerLevel)
	{
		subsystemTerrain = project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		if (subsystemTerrain == null || !(actual is FormattedRecipe formattedRecipe2))
		{
			formattedRecipe = null;
			ingredients = new string[36];
			heatLevel = 0f;
			playerLevel = 1f;
			return false;
		}
		formattedRecipe = formattedRecipe2;
		heatLevel = formattedRecipe2.RequiredHeatLevel;
		playerLevel = formattedRecipe2.RequiredPlayerLevel;
		ingredients = GetCraftingIngredientStrings(formattedRecipe2);
		return true;
	}
}
