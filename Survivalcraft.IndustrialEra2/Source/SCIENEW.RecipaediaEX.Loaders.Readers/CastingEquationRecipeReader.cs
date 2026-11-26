using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Engine;
using Game;
using RecipaediaEX;
using SCIENEW.RecipaediaEX.Recipes;
using SCIENEW.Utils;
using XmlUtilities;

namespace SCIENEW.RecipaediaEX.Loaders.Readers;

public class CastingEquationRecipeReader : IRecipesReader
{
	public string Identifier => "CastingEquationRecipeReader";

	public IRecipe? Decode(XElement element)
	{
		try
		{
			string attributeValue = XmlUtils.GetAttributeValue(element, "Description", string.Empty);
			float attributeValue2 = XmlUtils.GetAttributeValue(element, "RequiredPlayerLevel", 0f);
			int attributeValue3 = XmlUtils.GetAttributeValue(element, "DisplayOrder", 0);
			int attributeValue4 = XmlUtils.GetAttributeValue(element, "MatchPriority", 0);
			string attributeValue5 = XmlUtils.GetAttributeValue(element, "ProcessType", "BlastCasting");
			var (ingredientFluidValue, ingredientFluidVolume) = ParseIngredientFluid(element);
			var (moduleValues, moduleCount) = ParseModule(element);
			var (resultValue, resultCount) = ParseResult(element);
			return new CastingEquationRecipe
			{
				IngredientFluidValue = ingredientFluidValue,
				IngredientFluidVolume = ingredientFluidVolume,
				ModuleValues = moduleValues,
				ModuleCount = moduleCount,
				ResultValue = resultValue,
				ResultCount = resultCount,
				ProcessType = attributeValue5,
				RequiredPlayerLevel = attributeValue2,
				DisplayOrder = attributeValue3,
				MatchPriority = attributeValue4,
				Description = DecodeDescription(attributeValue)
			};
		}
		catch (Exception e)
		{
			Log.Warning("[CastingEquationRecipeReader] Decode recipe failed:");
			Log.Warning(element.ToString());
			Log.Error(e);
			return null;
		}
	}

	private static (int ingredientFluidValue, float ingredientFluidVolume) ParseIngredientFluid(XElement element)
	{
		string attributeValue = XmlUtils.GetAttributeValue(element, "IngredientFluid", string.Empty);
		if (!string.IsNullOrEmpty(attributeValue))
		{
			return (ingredientFluidValue: FluidsManager.GetFluidID(attributeValue, 0), ingredientFluidVolume: XmlUtils.GetAttributeValue(element, "IngredientFluidVolume", 0f));
		}
		EquationFluid[] array = ParseEquationFluids(element.Element("MaterialsFluids"));
		if (array.Length != 1)
		{
			throw new InvalidOperationException("[CastingEquationRecipeReader] MaterialsFluids must contain exactly one entry.");
		}
		return (ingredientFluidValue: array[0].Value, ingredientFluidVolume: array[0].Volume);
	}

	private static (int[] moduleValues, int moduleCount) ParseModule(XElement element)
	{
		string attributeValue = XmlUtils.GetAttributeValue(element, "Module", string.Empty);
		if (!string.IsNullOrEmpty(attributeValue))
		{
			return (moduleValues: new int[1] { DecodeDeterministicIngredientValue(attributeValue) }, moduleCount: XmlUtils.GetAttributeValue(element, "ModuleCount", 1));
		}
		EquationItem[] array = ParseEquationItems(element.Element("MaterialsItems"));
		if (array.Length == 0)
		{
			return (moduleValues: Array.Empty<int>(), moduleCount: 0);
		}
		if (array.Length > 1)
		{
			throw new InvalidOperationException("[CastingEquationRecipeReader] MaterialsItems supports at most one entry.");
		}
		return (moduleValues: array[0].Values ?? Array.Empty<int>(), moduleCount: array[0].Count);
	}

	private static EquationItem[] ParseEquationItems(XElement? itemsNode)
	{
		if (itemsNode == null)
		{
			return Array.Empty<EquationItem>();
		}
		List<EquationItem> list = new List<EquationItem>();
		foreach (XAttribute item in itemsNode.Attributes())
		{
			if (item.Name.LocalName.Length != 1 || !char.IsLower(item.Name.LocalName[0]))
			{
				continue;
			}
			string[] array = item.Value.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length != 2)
			{
				throw new InvalidOperationException("Invalid Equation item entry \"" + item.Value + "\".");
			}
			int count = int.Parse(array[1], CultureInfo.InvariantCulture);
			RecipesUtils.DecodeIEIngredient(array[0], out string craftingId, out int? data);
			int[] array2 = RecipesUtils.FindBlocksByCraftingId(craftingId, data);
			if (array2.Length == 0)
			{
				throw new InvalidOperationException("Block with craftingId \"" + array[0] + "\" not found.");
			}
			if (data.HasValue)
			{
				array2 = array2.Select((int v) => Terrain.ReplaceData(v, data.Value)).ToArray();
			}
			list.Add(new EquationItem(array2, count));
		}
		return list.ToArray();
	}

	private static (int resultValue, int resultCount) ParseResult(XElement element)
	{
		string attributeValue = XmlUtils.GetAttributeValue(element, "Result", string.Empty);
		if (!string.IsNullOrEmpty(attributeValue))
		{
			return (resultValue: DecodeDeterministicIngredientValue(attributeValue), resultCount: XmlUtils.GetAttributeValue(element, "ResultCount", 1));
		}
		EquationItem[] array = ParseEquationItems(element.Element("ResultsItems"));
		if (array.Length != 1 || array[0].Values == null || array[0].Values.Length != 1)
		{
			throw new InvalidOperationException("[CastingEquationRecipeReader] ResultsItems must contain exactly one deterministic entry.");
		}
		return (resultValue: array[0].Values[0], resultCount: array[0].Count);
	}

	private static int DecodeDeterministicIngredientValue(string ingredient)
	{
		RecipesUtils.DecodeIEIngredient(ingredient, out string craftingId, out int? data);
		int[] array = RecipesUtils.FindBlocksByCraftingId(craftingId, data);
		if (array.Length != 1)
		{
			throw new InvalidOperationException("Casting value \"" + ingredient + "\" must resolve to exactly one block value.");
		}
		if (!data.HasValue)
		{
			return array[0];
		}
		return Terrain.ReplaceData(array[0], data.Value);
	}

	private static EquationFluid[] ParseEquationFluids(XElement? fluidsNode)
	{
		if (fluidsNode == null)
		{
			return Array.Empty<EquationFluid>();
		}
		List<EquationFluid> list = new List<EquationFluid>();
		foreach (XAttribute item in fluidsNode.Attributes())
		{
			if (item.Name.LocalName.Length == 1 && char.IsLower(item.Name.LocalName[0]))
			{
				string[] array = item.Value.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				if (array.Length != 2)
				{
					throw new InvalidOperationException("Invalid Equation fluid entry \"" + item.Value + "\".");
				}
				float volume = float.Parse(array[1], CultureInfo.InvariantCulture);
				int fluidID = FluidsManager.GetFluidID(array[0], 0);
				list.Add(new EquationFluid(fluidID, volume));
			}
		}
		return list.ToArray();
	}

	private static string DecodeDescription(string description)
	{
		if (string.IsNullOrEmpty(description))
		{
			return description;
		}
		if (description.StartsWith('[') && description.EndsWith(']'))
		{
			string text = description.Substring(1, description.Length - 2);
			return LanguageControl.Get("Equations", "Descriptions", text);
		}
		return description;
	}
}
