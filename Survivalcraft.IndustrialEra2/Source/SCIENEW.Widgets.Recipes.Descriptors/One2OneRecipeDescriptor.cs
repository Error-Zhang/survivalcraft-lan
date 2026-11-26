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

[RecipeDescriptor(new Type[] { typeof(One2OneRecipe) }, 0)]
public class One2OneRecipeDescriptor : RecipeDescriptor
{
	public LabelWidget m_nameWidget;

	public LabelWidget m_descriptionWidget;

	public BlockRecipeSlotWidget m_ingredientWidget;

	public BlockRecipeSlotWidget m_resultWidget;

	public FireWidget m_fireWidget;

	public CanvasWidget m_smeltingWidget;

	public One2OneRecipeDescriptor(IRecipaediaRecipeNavigator navigator)
		: base(navigator)
	{
		XElement node = ContentManager.Get<XElement>("Widgets/RecipeDescriptors/One2OneRecipeDescriptor");
		LoadContents(this, node);
		m_nameWidget = Children.Find<LabelWidget>("One2OneRecipeDescriptor.Name");
		m_descriptionWidget = Children.Find<LabelWidget>("One2OneRecipeDescriptor.Description");
		m_ingredientWidget = Children.Find<BlockRecipeSlotWidget>("One2OneRecipeDescriptor.Ingredient");
		m_resultWidget = Children.Find<BlockRecipeSlotWidget>("One2OneRecipeDescriptor.Result");
		m_fireWidget = Children.Find<FireWidget>("One2OneRecipeDescriptor.Fire");
		m_smeltingWidget = Children.Find<CanvasWidget>("One2OneRecipeDescriptor.Smelting");
	}

	public override void Show(IRecipe recipe, string nameSuffix)
	{
		if (recipe is One2OneRecipe one2OneRecipe)
		{
			Block block = BlocksManager.Blocks[Terrain.ExtractContents(one2OneRecipe.ResultID)];
			m_nameWidget.Text = block.GetDisplayName(null, one2OneRecipe.ResultID) + ((!string.IsNullOrEmpty(nameSuffix)) ? nameSuffix : string.Empty);
			m_descriptionWidget.Text = one2OneRecipe.Description;
			m_nameWidget.IsVisible = true;
			m_descriptionWidget.IsVisible = true;
			CraftingRecipesManager.DecodeIngredient(one2OneRecipe.Ingredient, out var craftingId, out var data);
			ValueEnumerable<FromArray<int>, int> source = RecipesUtils.FindBlocksByCraftingId(craftingId, data).AsValueEnumerable();
			m_ingredientWidget.SetIngredients(source.Select((int blockValue) => new BlockItem(BlocksManager.Blocks[Terrain.ExtractContents(blockValue)], 0, blockValue)).OfType<IRecipaediaItem>().ToArray<ArraySelect<int, BlockItem>, BlockItem, IRecipaediaItem>(), m_navigator);
			m_resultWidget.SetResult(new BlockItem(BlocksManager.Blocks[Terrain.ExtractContents(one2OneRecipe.ResultID)], 0, one2OneRecipe.ResultID), m_navigator, one2OneRecipe.ResultCount);
			m_smeltingWidget.IsVisible = one2OneRecipe.Smelting;
			m_fireWidget.ParticlesPerSecond = 40f;
		}
	}

	public override void Hide()
	{
		m_nameWidget.IsVisible = false;
		m_descriptionWidget.IsVisible = false;
		m_ingredientWidget.ClearContents();
		m_resultWidget.ClearContents();
		m_smeltingWidget.IsVisible = false;
		m_fireWidget.ParticlesPerSecond = 0f;
	}

	public override CrafterButtonWidget GetCrafterButton(IRecipe recipe)
	{
		return CrafterButtonWidget.GetDefaultWidget(recipe, m_navigator);
	}
}
