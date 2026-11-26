using RecipaediaEX;
using TemplatesDatabase;

namespace SCIENEW.RecipaediaEX.Recipes;

public class MoltingEquationRecipe : IRecipe
{
	private readonly ValuesDictionary m_extraValues = new ValuesDictionary();

	public int DisplayOrder { get; init; }

	public int MatchPriority { get; init; }

	public float RequiredPlayerLevel { get; init; }

	public string Description { get; init; } = string.Empty;

	public int IngredientValue { get; init; }

	public int IngredientCount { get; init; } = 1;

	public int ResultFluidValue { get; init; }

	public float ResultFluidVolume { get; init; }

	public bool Match(IRecipe actual)
	{
		if (!(actual is MoltingEquationRecipe moltingEquationRecipe))
		{
			return false;
		}
		if (moltingEquationRecipe.RequiredPlayerLevel < RequiredPlayerLevel)
		{
			return false;
		}
		if (moltingEquationRecipe.IngredientValue == IngredientValue)
		{
			return moltingEquationRecipe.IngredientCount >= IngredientCount;
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
