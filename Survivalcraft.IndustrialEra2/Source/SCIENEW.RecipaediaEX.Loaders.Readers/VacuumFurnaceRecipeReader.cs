using System.Xml.Linq;
using RecipaediaEX;
using SCIENEW.RecipaediaEX.Recipes;
using SCIENEW.Utils;
using XmlUtilities;

namespace SCIENEW.RecipaediaEX.Loaders.Readers;

public class VacuumFurnaceRecipeReader : IRecipesReader
{
	public string Identifier => "VacuumFurnaceRecipeReader";

	public IRecipe? Decode(XElement element)
	{
		string attributeValue = XmlUtils.GetAttributeValue(element, "Description", string.Empty);
		string attributeValue2 = XmlUtils.GetAttributeValue<string>(element, "Result");
		int attributeValue3 = XmlUtils.GetAttributeValue(element, "ResultCount", 1);
		string attributeValue4 = XmlUtils.GetAttributeValue(element, "Remains", string.Empty);
		int attributeValue5 = XmlUtils.GetAttributeValue(element, "RemainsCount", 0);
		string attributeValue6 = XmlUtils.GetAttributeValue(element, "Ingredient1", XmlUtils.GetAttributeValue(element, "a", string.Empty));
		string attributeValue7 = XmlUtils.GetAttributeValue(element, "Ingredient2", XmlUtils.GetAttributeValue(element, "b", string.Empty));
		string attributeValue8 = XmlUtils.GetAttributeValue(element, "Ingredient3", XmlUtils.GetAttributeValue(element, "c", string.Empty));
		string attributeValue9 = XmlUtils.GetAttributeValue(element, "Ingredient4", XmlUtils.GetAttributeValue(element, "d", string.Empty));
		return new VacuumFurnaceRecipe
		{
			ResultID = RecipesUtils.DecodeIEResult(attributeValue2),
			ResultCount = attributeValue3,
			RemainsID = ((!string.IsNullOrEmpty(attributeValue4)) ? RecipesUtils.DecodeIEResult(attributeValue4) : 0),
			RemainsCount = attributeValue5,
			Ingredient1 = attributeValue6,
			Ingredient2 = attributeValue7,
			Ingredient3 = attributeValue8,
			Ingredient4 = attributeValue9,
			RequiredPlayerLevel = XmlUtils.GetAttributeValue(element, "RequiredPlayerLevel", 1f),
			DisplayOrder = XmlUtils.GetAttributeValue(element, "DisplayOrder", 0),
			MatchPriority = XmlUtils.GetAttributeValue(element, "MatchPriority", 0),
			Description = ((attributeValue.StartsWith('[') && attributeValue.EndsWith(']')) ? LanguageUtils.GetRecipeDescription(attributeValue) : attributeValue)
		};
	}
}
