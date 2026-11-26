using System;
using System.Collections.Generic;
using System.Globalization;
using Engine;
using Game;
using RecipaediaEX;
using RecipaediaEX.UI;
using ZLinq;

namespace SCIENEW.RecipaediaEX.Items;

public class FluidItem : IRecipaediaItem, IRecipaediaRecipeItem, IRecipaediaDescriptionItem
{
	public const string PropertiesLangSection = "FluidRecipaediaDescriptionScreen";

	public int m_fluidValue;

	public Fluid m_fluid;

	public int RecipesCount => RecipaediaEXManager.Recipes.AsValueEnumerable().Count(Match);

	public object Value => m_fluidValue;

	public string Name => m_fluid.GetDisplayName(m_fluidValue);

	public Widget Icon => new FluidWidget
	{
		m_fluidValue = m_fluidValue,
		m_relativeVolume = 1f,
		Size = new Vector2(96f, 96f),
		HorizontalAlignment = WidgetAlignment.Center,
		VerticalAlignment = WidgetAlignment.Center
	};

	public string Description => m_fluid.GetDescription(m_fluidValue);

	public string DetailScreenName => "FluidRecipaediaDescription";

	public string RecipeScreenName => "RecipaediaRecipes";

	public bool RecipesButtonEnabled => RecipesCount > 0;

	public string RecipesButtonText
	{
		get
		{
			if (RecipesCount <= 0)
			{
				return LanguageControl.Get("RecipaediaScreen", 3);
			}
			return $"{RecipesCount} {((RecipesCount == 1) ? LanguageControl.Get("RecipaediaScreen", 1) : LanguageControl.Get("RecipaediaScreen", 2))}";
		}
	}

	public bool DetailsButtonEnabled => true;

	public string DetailsButtonText => LanguageControl.Get("ContentWidgets", "RecipaediaScreen", "1");

	public FluidItem(int fluidValue, Fluid fluid)
	{
		m_fluidValue = fluidValue;
		m_fluid = fluid;
	}

	public Dictionary<string, string> GetProperties()
	{
		Dictionary<string, string> dictionary = BuildRawProperties(m_fluidValue, m_fluid);
		Dictionary<string, string> dictionary2 = new Dictionary<string, string>(dictionary.Count);
		foreach (KeyValuePair<string, string> item in dictionary)
		{
			dictionary2[LanguageControl.GetContentWidgets("FluidRecipaediaDescriptionScreen", item.Key)] = item.Value;
		}
		return dictionary2;
	}

	public static Dictionary<string, string> BuildRawProperties(int fluidValue, Fluid fluid)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (fluid is IFuelFluid fuelFluid)
		{
			if (fuelFluid.GetFuelLevel(fluidValue) > 0f)
			{
				dictionary.Add("FuelLevel", fuelFluid.GetFuelLevel(fluidValue).ToString(CultureInfo.InvariantCulture));
			}
			if (fuelFluid.GetFuelSpeed(fluidValue) > 0f)
			{
				dictionary.Add("FuelSpeed", fuelFluid.GetFuelSpeed(fluidValue).ToString(CultureInfo.InvariantCulture));
			}
		}
		dictionary.Add("Pipeable", (fluid is IPipeableFluid) ? LanguageControl.Yes : LanguageControl.No);
		dictionary.Add("TroughTransportable", (fluid is ITroughTransportableFluid) ? LanguageControl.Yes : LanguageControl.No);
		dictionary.Add("Scoopable", (fluid is IScoopableFluid) ? LanguageControl.Yes : LanguageControl.No);
		return dictionary;
	}

	public bool Match(IRecipe recipe)
	{
		try
		{
			return recipe.GetExtraValue("SCIENEW.MatchedResultFluidValues", Array.Empty<int>()).AsValueEnumerable().Contains(m_fluidValue);
		}
		catch (Exception ex)
		{
			Log.Error("FluidItem.Match error, probably because the problem of IRecipe.GetExtraValue(\"SCIENEW.MatchedResultFluidValues\"): " + ex);
			return false;
		}
	}

	public bool IsIngredient(IRecipe recipe)
	{
		return false;
	}
}
