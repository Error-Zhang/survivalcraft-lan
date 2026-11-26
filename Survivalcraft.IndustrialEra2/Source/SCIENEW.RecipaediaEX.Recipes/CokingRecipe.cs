using RecipaediaEX;
using SCIENEW.Utils;
using TemplatesDatabase;

namespace SCIENEW.RecipaediaEX.Recipes;

public class CokingRecipe : IRecipe
{
	private readonly ValuesDictionary m_extraValues = new ValuesDictionary();

	public int DisplayOrder { get; init; }

	public int MatchPriority { get; init; }

	public string Ingredient { get; init; } = string.Empty;

	public int IngredientCount { get; init; }

	public int ResultID { get; init; }

	public int ResultCount { get; init; }

	public string Description { get; init; } = string.Empty;

	public bool Match(IRecipe actual)
	{
		if (!(actual is CokingRecipe cokingRecipe))
		{
			return false;
		}
		if (!RecipesUtils.CompareIngredients(Ingredient, cokingRecipe.Ingredient))
		{
			return false;
		}
		return cokingRecipe.IngredientCount >= IngredientCount;
	}

	public T GetExtraValue<T>(string key, T defaultValue)
	{
		return m_extraValues.GetValue(key, defaultValue);
	}

	public void SetExtraValue<T>(string key, T value)
	{
		m_extraValues.SetValue(key, value);
	}
}
