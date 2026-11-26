using System.Xml.Linq;
using RecipaediaEX;
using SCIENEW.RecipaediaEX.Recipes;
using SCIENEW.Utils;
using XmlUtilities;

namespace SCIENEW.RecipaediaEX.Loaders.Readers;

public class FillingRecipeReader : IRecipesReader
{
	public string Identifier => "FillingRecipeReader";

	public IRecipe? Decode(XElement element)
	{
		string attributeValue = XmlUtils.GetAttributeValue(element, "Description", string.Empty);
		string attributeValue2 = XmlUtils.GetAttributeValue<string>(element, "Result");
		int attributeValue3 = XmlUtils.GetAttributeValue(element, "ResultCount", 1);
		string attributeValue4 = XmlUtils.GetAttributeValue<string>(element, "Ingredient");
		string attributeValue5 = XmlUtils.GetAttributeValue<string>(element, "CanIngredient");
		return new FillingRecipe
		{
			ResultID = RecipesUtils.DecodeIEResult(attributeValue2),
			ResultCount = attributeValue3,
			Ingredient = attributeValue4,
			CanIngredient = attributeValue5,
			RequiredPlayerLevel = XmlUtils.GetAttributeValue(element, "RequiredPlayerLevel", 1f),
			DisplayOrder = XmlUtils.GetAttributeValue(element, "DisplayOrder", 0),
			MatchPriority = XmlUtils.GetAttributeValue(element, "MatchPriority", 0),
			Description = ((attributeValue.StartsWith('[') && attributeValue.EndsWith(']')) ? LanguageUtils.GetRecipeDescription(attributeValue) : attributeValue)
		};
	}
}
