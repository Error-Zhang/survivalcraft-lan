using System;
using System.Collections.Generic;
using RecipaediaEX;
using TemplatesDatabase;

namespace SCIENEW.RecipaediaEX.Recipes;

public class CastingEquationRecipe : IRecipe
{
	private readonly ValuesDictionary m_extraValues = new ValuesDictionary();

	public int DisplayOrder { get; init; }

	public int MatchPriority { get; init; }

	public float RequiredPlayerLevel { get; init; }

	public string Description { get; init; } = string.Empty;

	public string ProcessType { get; init; } = "BlastCasting";

	public int IngredientFluidValue { get; init; }

	public float IngredientFluidVolume { get; init; }

	public int[] ModuleValues { get; init; } = Array.Empty<int>();

	public int ModuleCount { get; init; }

	public int ResultValue { get; init; }

	public int ResultCount { get; init; } = 1;

	public bool Match(IRecipe actual)
	{
		if (!(actual is CastingEquationRecipe castingEquationRecipe))
		{
			return false;
		}
		if (castingEquationRecipe.RequiredPlayerLevel < RequiredPlayerLevel)
		{
			return false;
		}
		if (castingEquationRecipe.ProcessType != ProcessType)
		{
			return false;
		}
		if (castingEquationRecipe.IngredientFluidValue != IngredientFluidValue || castingEquationRecipe.IngredientFluidVolume < IngredientFluidVolume)
		{
			return false;
		}
		if (ModuleCount <= 0)
		{
			if (castingEquationRecipe.ModuleCount > 0)
			{
				return castingEquationRecipe.ModuleValues.Length == 0;
			}
			return true;
		}
		if (castingEquationRecipe.ModuleCount < ModuleCount)
		{
			return false;
		}
		return ContainsUnorderedSubset(ModuleValues, castingEquationRecipe.ModuleValues);
	}

	public T GetExtraValue<T>(string key, T defaultValue)
	{
		return m_extraValues.GetValue(key, defaultValue);
	}

	public void SetExtraValue<T>(string key, T value)
	{
		m_extraValues.SetValue(key, value);
	}

	private static bool ContainsUnorderedSubset(int[] array, int[] subset)
	{
		List<int> list = new List<int>(array);
		foreach (int item in subset)
		{
			if (!list.Remove(item))
			{
				return false;
			}
		}
		return true;
	}
}
