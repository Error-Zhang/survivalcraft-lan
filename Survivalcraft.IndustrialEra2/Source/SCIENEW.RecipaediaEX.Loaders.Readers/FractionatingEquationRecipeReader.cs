using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using Engine;
using Game;
using RecipaediaEX;
using SCIENEW.RecipaediaEX.Recipes;
using XmlUtilities;

namespace SCIENEW.RecipaediaEX.Loaders.Readers;

public class FractionatingEquationRecipeReader : IRecipesReader
{
	public string Identifier => "FractionatingEquationRecipeReader";

	public IRecipe? Decode(XElement element)
	{
		try
		{
			string attributeValue = XmlUtils.GetAttributeValue(element, "Description", string.Empty);
			float attributeValue2 = XmlUtils.GetAttributeValue(element, "RequiredPlayerLevel", 0f);
			int attributeValue3 = XmlUtils.GetAttributeValue(element, "DisplayOrder", 0);
			int attributeValue4 = XmlUtils.GetAttributeValue(element, "MatchPriority", 0);
			return new FractionatingEquationRecipe
			{
				MaterialsFluids = ParseEquationFluids(element.Element("MaterialsFluids")),
				ResultsFluids = ParseEquationFluids(element.Element("ResultsFluids")),
				RequiredPlayerLevel = attributeValue2,
				DisplayOrder = attributeValue3,
				MatchPriority = attributeValue4,
				Description = DecodeDescription(attributeValue)
			};
		}
		catch (Exception e)
		{
			Log.Warning("[FractionatingEquationRecipeReader] Decode recipe failed:");
			Log.Warning(element.ToString());
			Log.Error(e);
			return null;
		}
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
