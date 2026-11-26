using System;
using System.Collections.Generic;
using RecipaediaEX;
using TemplatesDatabase;

namespace SCIENEW.RecipaediaEX.Recipes;

public class ReactionEquationRecipe : IRecipe
{
	private readonly ValuesDictionary m_extraValues = new ValuesDictionary();

	public int DisplayOrder { get; init; }

	public int MatchPriority { get; init; }

	public float RequiredHeatLevel { get; init; }

	public float RequiredPlayerLevel { get; init; }

	public string Description { get; init; } = string.Empty;

	public EquationItem[] MaterialsItems { get; init; } = Array.Empty<EquationItem>();

	public EquationFluid[] MaterialsFluids { get; init; } = Array.Empty<EquationFluid>();

	public EquationItem[] ResultsItems { get; init; } = Array.Empty<EquationItem>();

	public EquationFluid[] ResultsFluids { get; init; } = Array.Empty<EquationFluid>();

	public bool Match(IRecipe actual)
	{
		if (!(actual is ReactionEquationRecipe reactionEquationRecipe))
		{
			return false;
		}
		if (reactionEquationRecipe.RequiredPlayerLevel < RequiredPlayerLevel)
		{
			return false;
		}
		if (MatchItems(MaterialsItems, reactionEquationRecipe.MaterialsItems))
		{
			return MatchFluids(MaterialsFluids, reactionEquationRecipe.MaterialsFluids);
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

	private static bool MatchItems(EquationItem[] equationItems, EquationItem[] materialsItems)
	{
		if (equationItems.Length != materialsItems.Length)
		{
			return false;
		}
		List<EquationItem> list = new List<EquationItem>(equationItems);
		for (int i = 0; i < materialsItems.Length; i++)
		{
			EquationItem materialItem = materialsItems[i];
			int num = list.FindIndex((EquationItem equationItem) => ContainsUnorderedSubset(equationItem.Values ?? Array.Empty<int>(), materialItem.Values ?? Array.Empty<int>()) && equationItem.Count <= materialItem.Count);
			if (num < 0)
			{
				return false;
			}
			list.RemoveAt(num);
		}
		return true;
	}

	private static bool MatchFluids(EquationFluid[] equationFluids, EquationFluid[] materialsFluids)
	{
		if (equationFluids.Length != materialsFluids.Length)
		{
			return false;
		}
		List<EquationFluid> list = new List<EquationFluid>(equationFluids);
		for (int i = 0; i < materialsFluids.Length; i++)
		{
			EquationFluid materialFluid = materialsFluids[i];
			int num = list.FindIndex((EquationFluid equationFluid) => equationFluid.Value == materialFluid.Value && equationFluid.Volume <= materialFluid.Volume);
			if (num < 0)
			{
				return false;
			}
			list.RemoveAt(num);
		}
		return true;
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
