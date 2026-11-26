using System;
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

public class FermentingEquationRecipeReader : IRecipesReader
{
	public string Identifier => "FermentingEquationRecipeReader";

	public IRecipe? Decode(XElement element)
	{
		try
		{
			string attributeValue = XmlUtils.GetAttributeValue(element, "Description", string.Empty);
			float attributeValue2 = XmlUtils.GetAttributeValue(element, "RequiredPlayerLevel", 0f);
			int attributeValue3 = XmlUtils.GetAttributeValue(element, "DisplayOrder", 0);
			int attributeValue4 = XmlUtils.GetAttributeValue(element, "MatchPriority", 0);
			var (ingredientValue, ingredientCount) = ParseIngredient(element);
			var (resultFluidValue, resultFluidVolume) = ParseResultFluid(element);
			return new FermentingEquationRecipe
			{
				IngredientValue = ingredientValue,
				IngredientCount = ingredientCount,
				ResultFluidValue = resultFluidValue,
				ResultFluidVolume = resultFluidVolume,
				RequiredPlayerLevel = attributeValue2,
				DisplayOrder = attributeValue3,
				MatchPriority = attributeValue4,
				Description = DecodeDescription(attributeValue)
			};
		}
		catch (Exception e)
		{
			Log.Warning("[FermentingEquationRecipeReader] Decode recipe failed:");
			Log.Warning(element.ToString());
			Log.Error(e);
			return null;
		}
	}

	private static (int ingredientValue, int ingredientCount) ParseIngredient(XElement element)
	{
		string attributeValue = XmlUtils.GetAttributeValue(element, "Ingredient", string.Empty);
		if (!string.IsNullOrEmpty(attributeValue))
		{
			return (ingredientValue: DecodeDeterministicIngredientValue(attributeValue), ingredientCount: XmlUtils.GetAttributeValue(element, "IngredientCount", 1));
		}
		XAttribute xAttribute = (element.Element("MaterialsItems") ?? throw new InvalidOperationException("Fermenting equation requires MaterialsItems node.")).Attributes().FirstOrDefault((XAttribute a) => a.Name.LocalName.Length == 1 && char.IsLower(a.Name.LocalName[0]));
		if (xAttribute == null)
		{
			throw new InvalidOperationException("Fermenting equation requires one material item.");
		}
		string[] array = xAttribute.Value.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length != 2)
		{
			throw new InvalidOperationException("Invalid Equation item entry \"" + xAttribute.Value + "\".");
		}
		int item = int.Parse(array[1], CultureInfo.InvariantCulture);
		RecipesUtils.DecodeIEIngredient(array[0], out string craftingId, out int? data);
		int[] array2 = RecipesUtils.FindBlocksByCraftingId(craftingId, data);
		if (array2.Length != 1)
		{
			throw new InvalidOperationException("Fermenting ingredient \"" + array[0] + "\" must resolve to exactly one block value.");
		}
		return (ingredientValue: data.HasValue ? Terrain.ReplaceData(array2[0], data.Value) : array2[0], ingredientCount: item);
	}

	private static int DecodeDeterministicIngredientValue(string ingredient)
	{
		RecipesUtils.DecodeIEIngredient(ingredient, out string craftingId, out int? data);
		int[] array = RecipesUtils.FindBlocksByCraftingId(craftingId, data);
		if (array.Length != 1)
		{
			throw new InvalidOperationException("Fermenting ingredient \"" + ingredient + "\" must resolve to exactly one block value.");
		}
		if (!data.HasValue)
		{
			return array[0];
		}
		return Terrain.ReplaceData(array[0], data.Value);
	}

	private static (int fluidValue, float volume) ParseResultFluid(XElement element)
	{
		string attributeValue = XmlUtils.GetAttributeValue(element, "ResultFluid", string.Empty);
		if (!string.IsNullOrEmpty(attributeValue))
		{
			return (fluidValue: FluidsManager.GetFluidID(attributeValue, 0), volume: XmlUtils.GetAttributeValue(element, "ResultFluidVolume", 0f));
		}
		XAttribute xAttribute = (element.Element("ResultsFluids") ?? throw new InvalidOperationException("Fermenting equation requires ResultsFluids node.")).Attributes().FirstOrDefault((XAttribute a) => a.Name.LocalName.Length == 1 && char.IsLower(a.Name.LocalName[0]));
		if (xAttribute == null)
		{
			throw new InvalidOperationException("Fermenting equation requires one result fluid.");
		}
		string[] array = xAttribute.Value.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length != 2)
		{
			throw new InvalidOperationException("Invalid Equation fluid entry \"" + xAttribute.Value + "\".");
		}
		return new ValueTuple<int, float>(item2: float.Parse(array[1], CultureInfo.InvariantCulture), item1: FluidsManager.GetFluidID(array[0], 0));
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
