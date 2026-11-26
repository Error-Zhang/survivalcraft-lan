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

public class AlloyingEquationRecipeReader : IRecipesReader
{
	public string Identifier => "AlloyingEquationRecipeReader";

	public IRecipe? Decode(XElement element)
	{
		try
		{
			string attributeValue = XmlUtils.GetAttributeValue(element, "Description", string.Empty);
			float attributeValue2 = XmlUtils.GetAttributeValue(element, "RequiredPlayerLevel", 0f);
			int attributeValue3 = XmlUtils.GetAttributeValue(element, "DisplayOrder", 0);
			int attributeValue4 = XmlUtils.GetAttributeValue(element, "MatchPriority", 0);
			return new AlloyingEquationRecipe
			{
				MaterialsItems = ParseEquationItems(element.Element("MaterialsItems")),
				MaterialsFluids = ParseEquationFluids(element.Element("MaterialsFluids")),
				ResultsItems = ParseEquationItems(element.Element("ResultsItems")),
				RequiredPlayerLevel = attributeValue2,
				DisplayOrder = attributeValue3,
				MatchPriority = attributeValue4,
				Description = DecodeDescription(attributeValue)
			};
		}
		catch (Exception e)
		{
			Log.Warning("[AlloyingEquationRecipeReader] Decode recipe failed:");
			Log.Warning(element.ToString());
			Log.Error(e);
			return null;
		}
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
