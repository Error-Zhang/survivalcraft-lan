using RecipaediaEX;
using SCIENEW.Utils;
using TemplatesDatabase;

namespace SCIENEW.RecipaediaEX.Recipes;

public class FillingRecipe : IRecipe
{
	private readonly ValuesDictionary m_extraValues = new ValuesDictionary();

	public int DisplayOrder { get; init; }

	public int MatchPriority { get; init; }

	public int ResultID { get; init; }

	public int ResultCount { get; init; } = 1;

	public string Ingredient { get; init; } = string.Empty;

	public string CanIngredient { get; init; } = string.Empty;

	public float RequiredPlayerLevel { get; init; } = 1f;

	public string Description { get; init; } = string.Empty;

	public bool Match(IRecipe actual)
	{
		if (!(actual is FillingRecipe fillingRecipe))
		{
			return false;
		}
		if (RecipesUtils.CompareIngredients(Ingredient, fillingRecipe.Ingredient))
		{
			return RecipesUtils.CompareIngredients(CanIngredient, fillingRecipe.CanIngredient);
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
