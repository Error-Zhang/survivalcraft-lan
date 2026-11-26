using System;
using System.Collections.Generic;
using System.Xml.Linq;
using RecipaediaEX;
using SCIENEW.RecipaediaEX.Recipes;
using SCIENEW.Utils;
using XmlUtilities;

namespace SCIENEW.RecipaediaEX.Loaders.Readers;

public class SiftingRecipeReader : IRecipesReader
{
	public string Identifier => "SiftingRecipeReader";

	public IRecipe? Decode(XElement element)
	{
		string attributeValue = XmlUtils.GetAttributeValue(element, "Description", string.Empty);
		string attributeValue2 = XmlUtils.GetAttributeValue<string>(element, "Ingredient");
		int attributeValue3 = XmlUtils.GetAttributeValue(element, "IngredientCount", 1);
		string attributeValue4 = XmlUtils.GetAttributeValue(element, "ProcessType", "Sifting");
		int attributeValue5 = XmlUtils.GetAttributeValue(element, "DisplayOrder", 0);
		int attributeValue6 = XmlUtils.GetAttributeValue(element, "MatchPriority", 0);
		List<SiftingRecipe.SiftingResult> list = new List<SiftingRecipe.SiftingResult>();
		foreach (XElement item in element.Elements("Result"))
		{
			string attributeValue7 = XmlUtils.GetAttributeValue<string>(item, "Value");
			int attributeValue8 = XmlUtils.GetAttributeValue(item, "Count", 1);
			float attributeValue9 = XmlUtils.GetAttributeValue(item, "Weight", 1f);
			int value = RecipesUtils.DecodeIEResult(attributeValue7);
			list.Add(new SiftingRecipe.SiftingResult(value, attributeValue8, attributeValue9));
		}
		if (list.Count == 0)
		{
			throw new InvalidOperationException("SiftingRecipe requires at least one <Result /> child element.");
		}
		return new SiftingRecipe
		{
			Ingredient = attributeValue2,
			IngredientCount = attributeValue3,
			ProcessType = attributeValue4,
			Description = ((attributeValue.StartsWith('[') && attributeValue.EndsWith(']')) ? LanguageUtils.GetRecipeDescription(attributeValue) : attributeValue),
			DisplayOrder = attributeValue5,
			MatchPriority = attributeValue6,
			Results = list.ToArray()
		};
	}
}
