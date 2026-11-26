using System;
using RecipaediaEX;
using SCIENEW.Utils;
using TemplatesDatabase;

namespace SCIENEW.RecipaediaEX.Recipes;

public class SiftingRecipe : IRecipe
{
	public readonly struct SiftingResult(int value, int count, float weight)
	{
		public int Value { get; } = value;

		public int Count { get; } = count;

		public float Weight { get; } = weight;
	}

	private readonly ValuesDictionary m_extraValues = new ValuesDictionary();

	public int DisplayOrder { get; init; }

	public int MatchPriority { get; init; }

	public string Ingredient { get; init; } = string.Empty;

	public int IngredientCount { get; init; } = 1;

	public string ProcessType { get; init; } = "Sifting";

	public string Description { get; init; } = string.Empty;

	public SiftingResult[] Results
	{
		get;
		init
		{
			field = value ?? Array.Empty<SiftingResult>();
		}
	} = Array.Empty<SiftingResult>();

	public bool Match(IRecipe actual)
	{
		if (!(actual is SiftingRecipe siftingRecipe))
		{
			return false;
		}
		if (!RecipesUtils.CompareIngredients(Ingredient, siftingRecipe.Ingredient))
		{
			return false;
		}
		if (siftingRecipe.ProcessType == ProcessType)
		{
			return siftingRecipe.IngredientCount >= IngredientCount;
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
