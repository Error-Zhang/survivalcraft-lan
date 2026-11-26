using System.Xml.Linq;
using RecipaediaEX;
using SCIENEW.RecipaediaEX.Recipes;
using SCIENEW.Utils;
using XmlUtilities;

namespace SCIENEW.RecipaediaEX.Loaders.Readers;

public class WireComposingRecipeReader : IRecipesReader
{
	public string Identifier => "WireComposingRecipeReader";

	public IRecipe? Decode(XElement element)
	{
		string attributeValue = XmlUtils.GetAttributeValue<string>(element, "Description");
		string attributeValue2 = XmlUtils.GetAttributeValue<string>(element, "Result");
		int attributeValue3 = XmlUtils.GetAttributeValue(element, "ResultCount", 1);
		string attributeValue4 = XmlUtils.GetAttributeValue<string>(element, "Ingredient");
		int attributeValue5 = XmlUtils.GetAttributeValue(element, "IngredientCount", 1);
		string attributeValue6 = XmlUtils.GetAttributeValue<string>(element, "WireIngredient");
		int attributeValue7 = XmlUtils.GetAttributeValue(element, "WireIngredientCount", 1);
		return new WireComposingRecipe
		{
			ResultID = RecipesUtils.DecodeIEResult(attributeValue2),
			ResultCount = attributeValue3,
			Ingredient = attributeValue4,
			IngredientCount = attributeValue5,
			WireIngredient = attributeValue6,
			WireIngredientCount = attributeValue7,
			Description = ((attributeValue.StartsWith('[') && attributeValue.EndsWith(']')) ? LanguageUtils.GetRecipeDescription(attributeValue) : attributeValue)
		};
	}
}
