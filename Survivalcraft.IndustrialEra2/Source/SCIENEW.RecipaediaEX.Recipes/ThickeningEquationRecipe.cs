using RecipaediaEX;
using TemplatesDatabase;

namespace SCIENEW.RecipaediaEX.Recipes;

public class ThickeningEquationRecipe : IRecipe
{
	private readonly ValuesDictionary m_extraValues = new ValuesDictionary();

	public int DisplayOrder { get; init; }

	public int MatchPriority { get; init; }

	public float RequiredPlayerLevel { get; init; }

	public string Description { get; init; } = string.Empty;

	public int IngredientValue { get; init; }

	public int IngredientCount { get; init; } = 1;

	public int IngredientFluidValue { get; init; }

	public float IngredientFluidVolume { get; init; }

	public int ResultValue { get; init; }

	public int ResultCount { get; init; } = 1;

	public bool Match(IRecipe actual)
	{
		if (!(actual is ThickeningEquationRecipe thickeningEquationRecipe))
		{
			return false;
		}
		if (thickeningEquationRecipe.RequiredPlayerLevel < RequiredPlayerLevel)
		{
			return false;
		}
		if (thickeningEquationRecipe.IngredientValue == IngredientValue && thickeningEquationRecipe.IngredientCount >= IngredientCount && thickeningEquationRecipe.IngredientFluidValue == IngredientFluidValue)
		{
			return thickeningEquationRecipe.IngredientFluidVolume >= IngredientFluidVolume;
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
