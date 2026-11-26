using RecipaediaEX;
using SCIENEW.Utils;
using TemplatesDatabase;

namespace SCIENEW.RecipaediaEX.Recipes;

public class WireComposingRecipe : IRecipe
{
	private readonly ValuesDictionary m_extraValues = new ValuesDictionary();

	public int DisplayOrder => 0;

	public int MatchPriority => 0;

	public int ResultID { get; init; }

	public int ResultCount { get; init; }

	public string Ingredient { get; init; } = string.Empty;

	public int IngredientCount { get; init; } = 1;

	public string WireIngredient { get; init; } = string.Empty;

	public int WireIngredientCount { get; init; } = 1;

	public string Description { get; init; } = string.Empty;

	public bool Match(IRecipe actual)
	{
		if (!(actual is WireComposingRecipe wireComposingRecipe))
		{
			return false;
		}
		if (RecipesUtils.CompareIngredients(Ingredient, wireComposingRecipe.Ingredient) && RecipesUtils.CompareIngredients(WireIngredient, wireComposingRecipe.WireIngredient) && wireComposingRecipe.IngredientCount >= IngredientCount)
		{
			return wireComposingRecipe.WireIngredientCount >= WireIngredientCount;
		}
		return false;
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
