using System;
using System.Linq;
using Engine;
using RecipaediaEX;
using RecipaediaEX.Implementation;
using SCIENEW.Utils;
using ZLinq;

namespace SCIENEW.RecipaediaEX.Recipes;

public class IECraftingRecipe : OriginalCraftingRecipe
{
	public override bool Match(IRecipe actual)
	{
		if (!(actual is OriginalCraftingRecipe originalCraftingRecipe))
		{
			return false;
		}
		if (originalCraftingRecipe.RequiredHeatLevel != 0f)
		{
			return false;
		}
		if (originalCraftingRecipe.RequiredPlayerLevel < RequiredPlayerLevel)
		{
			return false;
		}
		return MatchIngredients(actual);
	}

	public override bool CompareIngredient(string requiredIngredient, string actualIngredient, bool throwOnNotSpecified = false)
	{
		return RecipesUtils.CompareIngredients(requiredIngredient, actualIngredient, throwOnNotSpecified);
	}

	public virtual bool MatchIngredients(IRecipe actual)
	{
		try
		{
			if (!(actual is FormattedRecipe formattedRecipe))
			{
				return false;
			}
			string?[] actualIngredients = (from x in formattedRecipe.GetExtraValue("ActualIngredients", Array.Empty<int?>()).AsEnumerable()
				select (!x.HasValue) ? null : RecipesUtils.BlockToCratingID(x.Value)).ToArray();
			return TransformedIngredients.AsValueEnumerable().Any((string[] ingredients) => CompareIngredientsArray(ingredients, actualIngredients));
		}
		catch (Exception ex)
		{
			if (LogErrorOnMatchFail)
			{
				Log.Error("[RecipaediaEX]Formatted Recipe Match Error: " + ex);
			}
			return false;
		}
	}
}
