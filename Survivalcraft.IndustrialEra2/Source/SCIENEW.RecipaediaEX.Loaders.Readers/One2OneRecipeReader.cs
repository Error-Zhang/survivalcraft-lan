using System.Xml.Linq;
using RecipaediaEX;
using RecipaediaEX.Implementation;
using SCIENEW.Extensions;
using SCIENEW.RecipaediaEX.Recipes;
using SCIENEW.Utils;
using XmlUtilities;

namespace SCIENEW.RecipaediaEX.Loaders.Readers;

public class One2OneRecipeReader : IRecipesReader
{
	private XmlRecipesLoader? xmlRecipesLoader;

	public string Identifier => "One2OneRecipeReader";

	public void SetParent(XmlRecipesLoader loader)
	{
		xmlRecipesLoader = loader;
	}

	public IRecipe? Decode(XElement element)
	{
		string attributeValue = XmlUtils.GetAttributeValue<string>(element, "Description");
		string attributeValue2 = element.GetAttributeValue<string>("Result");
		int attributeValue3 = element.GetAttributeValue<int>("ResultCount");
		string attributeValue4 = element.GetAttributeValue<string>("Ingredient");
		int attributeValue5 = element.GetAttributeValue<int>("IngredientCount");
		string attributeValue6 = element.GetAttributeValue<string>("ProcessType");
		bool attributeValue7 = element.GetAttributeValue("Smelting", defaultValue: false);
		return new One2OneRecipe
		{
			ResultID = RecipesUtils.DecodeIEResult(attributeValue2),
			ResultCount = attributeValue3,
			Ingredient = attributeValue4,
			IngredientCount = attributeValue5,
			ProcessType = attributeValue6,
			Smelting = attributeValue7,
			Description = ((attributeValue.StartsWith('[') && attributeValue.EndsWith(']')) ? LanguageUtils.GetRecipeDescription(attributeValue) : attributeValue)
		};
	}
}
