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

[RecipeDescriptor(new Type[] { typeof(WireComposingRecipe) }, 0)]
public class WireComposingRecipeDescriptor : RecipeDescriptor
{
	public LabelWidget m_nameWidget;

	public LabelWidget m_descriptionWidget;

	public BlockRecipeSlotWidget m_ingredientWidget;

	public BlockRecipeSlotWidget m_wireIngredientWidget;

	public BlockRecipeSlotWidget m_resultWidget;

	public WireComposingRecipeDescriptor(IRecipaediaRecipeNavigator navigator)
		: base(navigator)
	{
		XElement node = ContentManager.Get<XElement>("Widgets/RecipeDescriptors/WireComposingRecipeDescriptor");
		LoadContents(this, node);
		m_nameWidget = Children.Find<LabelWidget>("WireComposingRecipeDescriptor.Name");
		m_descriptionWidget = Children.Find<LabelWidget>("WireComposingRecipeDescriptor.Description");
		m_ingredientWidget = Children.Find<BlockRecipeSlotWidget>("WireComposingRecipeDescriptor.Ingredient");
		m_wireIngredientWidget = Children.Find<BlockRecipeSlotWidget>("WireComposingRecipeDescriptor.WireIngredient");
		m_resultWidget = Children.Find<BlockRecipeSlotWidget>("WireComposingRecipeDescriptor.Result");
	}

	public override void Show(IRecipe recipe, string nameSuffix)
	{
		if (recipe is WireComposingRecipe wireComposingRecipe)
		{
			Block block = BlocksManager.Blocks[Terrain.ExtractContents(wireComposingRecipe.ResultID)];
			m_nameWidget.Text = block.GetDisplayName(null, wireComposingRecipe.ResultID) + ((!string.IsNullOrEmpty(nameSuffix)) ? nameSuffix : string.Empty);
			m_descriptionWidget.Text = wireComposingRecipe.Description;
			m_nameWidget.IsVisible = true;
			m_descriptionWidget.IsVisible = true;
			SetIngredientSlot(m_ingredientWidget, wireComposingRecipe.Ingredient);
			SetIngredientSlot(m_wireIngredientWidget, wireComposingRecipe.WireIngredient);
			m_resultWidget.SetResult(new BlockItem(block, 0, wireComposingRecipe.ResultID), m_navigator, wireComposingRecipe.ResultCount);
		}
	}

	public override void Hide()
	{
		m_nameWidget.IsVisible = false;
		m_descriptionWidget.IsVisible = false;
		m_ingredientWidget.ClearContents();
		m_wireIngredientWidget.ClearContents();
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
