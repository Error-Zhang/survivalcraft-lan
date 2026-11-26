using RecipaediaEX;
using SCIENEW.Utils;
using TemplatesDatabase;

namespace SCIENEW.RecipaediaEX.Recipes;

public class One2OneRecipe : IRecipe
{
	private readonly ValuesDictionary m_extraValues = new ValuesDictionary();

	public int DisplayOrder => 0;

	public int MatchPriority => 0;

	public int ResultID { get; init; }

	public int ResultCount { get; init; }

	public string Ingredient { get; init; } = string.Empty;

	public int IngredientCount { get; init; }

	public string ProcessType { get; init; } = string.Empty;

	public bool Smelting { get; init; }

	public string Description { get; init; } = string.Empty;

	public bool Match(IRecipe actual)
	{
		if (!(actual is One2OneRecipe one2OneRecipe))
		{
			return false;
		}
		if (one2OneRecipe.ProcessType == ProcessType && RecipesUtils.CompareIngredients(Ingredient, one2OneRecipe.Ingredient))
		{
			return one2OneRecipe.IngredientCount >= IngredientCount;
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
