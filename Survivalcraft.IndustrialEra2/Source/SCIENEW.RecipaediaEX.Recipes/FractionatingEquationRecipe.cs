using System;
using System.Collections.Generic;
using RecipaediaEX;
using TemplatesDatabase;

namespace SCIENEW.RecipaediaEX.Recipes;

public class FractionatingEquationRecipe : IRecipe
{
	private readonly ValuesDictionary m_extraValues = new ValuesDictionary();

	public int DisplayOrder { get; init; }

	public int MatchPriority { get; init; }

	public float RequiredPlayerLevel { get; init; }

	public string Description { get; init; } = string.Empty;

	public EquationFluid[] MaterialsFluids { get; init; } = Array.Empty<EquationFluid>();

	public EquationFluid[] ResultsFluids { get; init; } = Array.Empty<EquationFluid>();

	public bool Match(IRecipe actual)
	{
		if (!(actual is FractionatingEquationRecipe fractionatingEquationRecipe))
		{
			return false;
		}
		if (fractionatingEquationRecipe.RequiredPlayerLevel < RequiredPlayerLevel)
		{
			return false;
		}
		return MatchFluids(MaterialsFluids, fractionatingEquationRecipe.MaterialsFluids);
	}

	public T GetExtraValue<T>(string key, T defaultValue)
	{
		return m_extraValues.GetValue(key, defaultValue);
	}

	public void SetExtraValue<T>(string key, T value)
	{
		m_extraValues.SetValue(key, value);
	}

	private static bool MatchFluids(EquationFluid[] equationFluids, EquationFluid[] fluids)
	{
		if (equationFluids.Length != fluids.Length)
		{
			return false;
		}
		List<EquationFluid> list = new List<EquationFluid>(equationFluids);
		for (int i = 0; i < fluids.Length; i++)
		{
			EquationFluid fluid = fluids[i];
			int num = list.FindIndex((EquationFluid equationFluid) => equationFluid.Value == fluid.Value && equationFluid.Volume <= fluid.Volume);
			if (num < 0)
			{
				return false;
			}
			list.RemoveAt(num);
		}
		return true;
	}
}
