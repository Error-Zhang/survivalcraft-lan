using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Game;
using RecipaediaEX;
using RecipaediaEX.Implementation;
using SCIENEW.RecipaediaEX.Recipes;
using SCIENEW.Utils;
using XmlUtilities;
using ZLinq;
using ZLinq.Linq;

namespace SCIENEW.RecipaediaEX.Loaders.Readers;

public class FormattedRecipeReader : IRecipesReader
{
	public string Identifier => "Default";

	public IRecipe? Decode(XElement element)
	{
		IRecipe recipe = ReadFormattedRecipe(element);
		if (recipe == null)
		{
			return null;
		}
		FormattedRecipe formattedRecipe = (FormattedRecipe)recipe;
		formattedRecipe.Description = LanguageUtils.GetRecipeDescription(formattedRecipe.Description);
		formattedRecipe.Message = LanguageUtils.GetRecipeMessage(formattedRecipe.Message);
		return recipe;
	}

	public virtual IRecipe ReadFormattedRecipe(XElement item)
	{
		float attributeValue = XmlUtils.GetAttributeValue<float>(item, "RequiredHeatLevel");
		FormattedRecipe formattedRecipe = ((attributeValue > 0f) ? ((FormattedRecipe)new IESmeltingRecipe()) : ((FormattedRecipe)new IECraftingRecipe()));
		string attributeValue2 = XmlUtils.GetAttributeValue<string>(item, "Result");
		string attributeValue3 = XmlUtils.GetAttributeValue<string>(item, "Description");
		formattedRecipe.Description = ((attributeValue3.StartsWith('[') && attributeValue3.EndsWith(']')) ? LanguageUtils.GetRecipeDescription(attributeValue3) : attributeValue3);
		formattedRecipe.ResultValue = RecipesUtils.DecodeIEResult(attributeValue2);
		formattedRecipe.ResultCount = XmlUtils.GetAttributeValue<int>(item, "ResultCount");
		string attributeValue4 = XmlUtils.GetAttributeValue(item, "Remains", string.Empty);
		if (!string.IsNullOrEmpty(attributeValue4))
		{
			formattedRecipe.RemainsValue = RecipesUtils.DecodeIEResult(attributeValue4);
			formattedRecipe.RemainsCount = XmlUtils.GetAttributeValue<int>(item, "RemainsCount");
		}
		formattedRecipe.RequiredHeatLevel = attributeValue;
		formattedRecipe.RequiredPlayerLevel = XmlUtils.GetAttributeValue(item, "RequiredPlayerLevel", 1f);
		formattedRecipe.Message = XmlUtils.GetAttributeValue<string>(item, "Message", null);
		if (!string.IsNullOrEmpty(formattedRecipe.Message))
		{
			formattedRecipe.Message = LanguageUtils.GetRecipeMessage(formattedRecipe.Message);
		}
		formattedRecipe.DisplayOrder = XmlUtils.GetAttributeValue(item, "DisplayOrder", 0);
		Dictionary<char, string> dictionary = new Dictionary<char, string>();
		ValueEnumerator<Where<FromEnumerable<XAttribute>, XAttribute>, XAttribute> enumerator = ValueEnumerableExtensions.GetEnumerator<Where<FromEnumerable<XAttribute>, XAttribute>, XAttribute>(in ILSpyHelper_AsRefReadOnly(from a in item.Attributes().AsValueEnumerable()
			where a.Name.LocalName.Length == 1 && char.IsLower(a.Name.LocalName[0])
			select a));
		try
		{
			while (enumerator.MoveNext())
			{
				XAttribute current = enumerator.Current;
				CraftingRecipesManager.DecodeIngredient(current.Value, out var craftingId, out var data);
				if (RecipesUtils.FindBlocksByCraftingId(craftingId, data).Length == 0)
				{
					throw new InvalidOperationException("Block with craftingId \"" + current.Value + "\" not found.");
				}
				if (data.HasValue && (data.Value < 0 || data.Value > 262143))
				{
					throw new InvalidOperationException("Data in recipe ingredient \"" + current.Value + "\" must be between 0 and 0x3FFFF.");
				}
				dictionary.Add(current.Name.LocalName[0], current.Value);
			}
		}
		finally
		{
			enumerator.Dispose();
		}
		string[] array = item.Value.Trim().Split(new string[1] { "\n" }, StringSplitOptions.None);
		for (int num = 0; num < array.Length; num++)
		{
			int num2 = array[num].IndexOf('"');
			int num3 = array[num].LastIndexOf('"');
			if (num2 < 0 || num3 < 0 || num3 <= num2)
			{
				throw new InvalidOperationException("Invalid recipe line.");
			}
			string text = array[num].Substring(num2 + 1, num3 - num2 - 1);
			for (int num4 = 0; num4 < text.Length; num4++)
			{
				char c = text[num4];
				if (char.IsLower(c))
				{
					string text2 = dictionary[c];
					formattedRecipe.Ingredients[num4 + num * 6] = text2;
				}
			}
		}
		formattedRecipe.PreTransformIngredients();
		return formattedRecipe;
		static ref readonly T ILSpyHelper_AsRefReadOnly<T>(in T temp) where T : allows ref struct
		{
			//ILSpy generated this function to help ensure overload resolution can pick the overload using 'in'
			return ref temp;
		}
	}
}
