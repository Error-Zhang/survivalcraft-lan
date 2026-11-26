using System;
using System.Xml.Linq;
using Game;
using RecipaediaEX;
using RecipaediaEX.Implementation;
using RecipaediaEX.UI;
using SCIENEW.RecipaediaEX.Recipes;
using SCIENEW.Utils;
using ZLinq;
using ZLinq.Linq;

namespace SCIENEW.Widgets.Recipes.Descriptors;

[RecipeDescriptor(new Type[] { typeof(FillingRecipe) }, 0)]
public class FillingRecipeDescriptor : RecipeDescriptor
{
	public LabelWidget m_nameWidget;

	public LabelWidget m_descriptionWidget;

	public BlockRecipeSlotWidget m_ingredientWidget;

	public BlockRecipeSlotWidget m_canWidget;

	public BlockRecipeSlotWidget m_resultWidget;

	public FillingRecipeDescriptor(IRecipaediaRecipeNavigator navigator)
		: base(navigator)
	{
		XElement node = ContentManager.Get<XElement>("Widgets/RecipeDescriptors/FillingRecipeDescriptor");
		LoadContents(this, node);
		m_nameWidget = Children.Find<LabelWidget>("FillingRecipeDescriptor.Name");
		m_descriptionWidget = Children.Find<LabelWidget>("FillingRecipeDescriptor.Description");
		m_ingredientWidget = Children.Find<BlockRecipeSlotWidget>("FillingRecipeDescriptor.Ingredient");
		m_canWidget = Children.Find<BlockRecipeSlotWidget>("FillingRecipeDescriptor.Can");
		m_resultWidget = Children.Find<BlockRecipeSlotWidget>("FillingRecipeDescriptor.Result");
	}

	public override void Show(IRecipe recipe, string nameSuffix)
	{
		if (recipe is FillingRecipe fillingRecipe)
		{
			Block block = BlocksManager.Blocks[Terrain.ExtractContents(fillingRecipe.ResultID)];
			m_nameWidget.Text = block.GetDisplayName(null, fillingRecipe.ResultID) + ((!string.IsNullOrEmpty(nameSuffix)) ? nameSuffix : string.Empty);
			m_descriptionWidget.Text = fillingRecipe.Description;
			m_nameWidget.IsVisible = true;
			m_descriptionWidget.IsVisible = true;
			SetIngredientSlot(m_ingredientWidget, fillingRecipe.Ingredient);
			SetIngredientSlot(m_canWidget, fillingRecipe.CanIngredient);
			m_resultWidget.SetResult(new BlockItem(block, 0, fillingRecipe.ResultID), m_navigator, fillingRecipe.ResultCount);
		}
	}

	public override void Hide()
	{
		m_nameWidget.IsVisible = false;
		m_descriptionWidget.IsVisible = false;
		m_ingredientWidget.ClearContents();
		m_canWidget.ClearContents();
		m_resultWidget.ClearContents();
	}

	public override CrafterButtonWidget GetCrafterButton(IRecipe recipe)
	{
		return CrafterButtonWidget.GetDefaultWidget(recipe, m_navigator);
	}

	private void SetIngredientSlot(BlockRecipeSlotWidget slot, string ingredient)
	{
		CraftingRecipesManager.DecodeIngredient(ingredient, out var craftingId, out var data);
		ValueEnumerable<FromArray<int>, int> source = RecipesUtils.FindBlocksByCraftingId(craftingId, data).AsValueEnumerable();
		slot.SetIngredients(source.Select((int blockValue) => new BlockItem(BlocksManager.Blocks[Terrain.ExtractContents(blockValue)], 0, blockValue)).OfType<IRecipaediaItem>().ToArray<ArraySelect<int, BlockItem>, BlockItem, IRecipaediaItem>(), m_navigator);
	}
}
