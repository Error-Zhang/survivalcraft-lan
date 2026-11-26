using System.Xml.Linq;
using RecipaediaEX;
using SCIENEW.RecipaediaEX.Recipes;
using SCIENEW.Utils;
using XmlUtilities;

namespace SCIENEW.RecipaediaEX.Loaders.Readers;

public class CokingRecipeReader : IRecipesReader
{
	public string Identifier => "CokingRecipeReader";

	public IRecipe? Decode(XElement element)
	{
		string attributeValue = XmlUtils.GetAttributeValue(element, "Description", string.Empty);
		string attributeValue2 = XmlUtils.GetAttributeValue<string>(element, "Ingredient");
		int attributeValue3 = XmlUtils.GetAttributeValue(element, "IngredientCount", 1);
		int attributeValue4 = XmlUtils.GetAttributeValue(element, "DisplayOrder", 0);
		int attributeValue5 = XmlUtils.GetAttributeValue(element, "MatchPriority", 0);
		string attributeValue6 = XmlUtils.GetAttributeValue<string>(element, "Result");
		int attributeValue7 = XmlUtils.GetAttributeValue(element, "ResultCount", 1);
		return new CokingRecipe
		{
			Ingredient = attributeValue2,
			IngredientCount = attributeValue3,
			Description = ((attributeValue.StartsWith('[') && attributeValue.EndsWith(']')) ? LanguageUtils.GetRecipeDescription(attributeValue) : attributeValue),
			DisplayOrder = attributeValue4,
			MatchPriority = attributeValue5,
			ResultID = RecipesUtils.DecodeIEResult(attributeValue6),
			ResultCount = attributeValue7
		};
	}
}
