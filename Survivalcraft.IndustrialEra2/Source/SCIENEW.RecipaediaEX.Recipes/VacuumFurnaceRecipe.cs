using RecipaediaEX;
using SCIENEW.Utils;
using TemplatesDatabase;

namespace SCIENEW.RecipaediaEX.Recipes;

public class VacuumFurnaceRecipe : IRecipe
{
	private readonly ValuesDictionary m_extraValues = new ValuesDictionary();

	public int DisplayOrder { get; init; }

	public int MatchPriority { get; init; }

	public int ResultID { get; init; }

	public int ResultCount { get; init; } = 1;

	public int RemainsID { get; init; }

	public int RemainsCount { get; init; }

	public string Ingredient1 { get; init; } = string.Empty;

	public string Ingredient2 { get; init; } = string.Empty;

	public string Ingredient3 { get; init; } = string.Empty;

	public string Ingredient4 { get; init; } = string.Empty;

	public float RequiredPlayerLevel { get; init; } = 1f;

	public string Description { get; init; } = string.Empty;

	public bool Match(IRecipe actual)
	{
		if (!(actual is VacuumFurnaceRecipe vacuumFurnaceRecipe))
		{
			return false;
		}
		if (CompareSlot(Ingredient1, vacuumFurnaceRecipe.Ingredient1) && CompareSlot(Ingredient2, vacuumFurnaceRecipe.Ingredient2) && CompareSlot(Ingredient3, vacuumFurnaceRecipe.Ingredient3))
		{
			return CompareSlot(Ingredient4, vacuumFurnaceRecipe.Ingredient4);
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

	private static bool CompareSlot(string required, string actual)
	{
		bool flag = string.IsNullOrEmpty(required);
		bool flag2 = string.IsNullOrEmpty(actual);
		if (flag | flag2)
		{
			return flag == flag2;
		}
		return RecipesUtils.CompareIngredients(required, actual);
	}
}
