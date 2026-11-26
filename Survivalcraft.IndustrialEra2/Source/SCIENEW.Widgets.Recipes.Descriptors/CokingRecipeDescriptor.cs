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

[RecipeDescriptor(new Type[] { typeof(CokingRecipe) }, 0)]
public class CokingRecipeDescriptor : RecipeDescriptor
{
	public LabelWidget m_nameWidget;

	public LabelWidget m_descriptionWidget;

	public BlockRecipeSlotWidget m_resultWidget;

	public BlockRecipeSlotWidget m_ingredientWidget;

	public FireWidget m_fireWidget;

	public CokingRecipeDescriptor(IRecipaediaRecipeNavigator navigator)
		: base(navigator)
	{
		XElement node = ContentManager.Get<XElement>("Widgets/RecipeDescriptors/CokingRecipeDescriptor");
		LoadContents(this, node);
		m_nameWidget = Children.Find<LabelWidget>("CokingRecipeDescriptor.Name");
		m_descriptionWidget = Children.Find<LabelWidget>("CokingRecipeDescriptor.Description");
		m_ingredientWidget = Children.Find<BlockRecipeSlotWidget>("CokingRecipeDescriptor.Ingredient");
		m_resultWidget = Children.Find<BlockRecipeSlotWidget>("CokingRecipeDescriptor.Result");
		m_fireWidget = Children.Find<FireWidget>("CokingRecipeDescriptor.Fire");
		m_ingredientWidget.IsCountVisible = (BlockRecipeSlotWidget _) => true;
		m_ingredientWidget.CountTextProcesser = null;
	}

	public override void Show(IRecipe recipe, string nameSuffix)
	{
		CokingRecipe cokingRecipe = recipe as CokingRecipe;
		if (cokingRecipe != null)
		{
			Block block = BlocksManager.Blocks[Terrain.ExtractContents(cokingRecipe.ResultID)];
			m_nameWidget.Text = block.GetDisplayName(null, cokingRecipe.ResultID) + ((!string.IsNullOrEmpty(nameSuffix)) ? nameSuffix : string.Empty);
			m_descriptionWidget.Text = cokingRecipe.Description;
			m_nameWidget.IsVisible = true;
			m_descriptionWidget.IsVisible = true;
			CraftingRecipesManager.DecodeIngredient(cokingRecipe.Ingredient, out var craftingId, out var data);
			ValueEnumerable<FromArray<int>, int> source = RecipesUtils.FindBlocksByCraftingId(craftingId, data).AsValueEnumerable();
			m_ingredientWidget.SetIngredients(source.Select((int blockValue) => new BlockItem(BlocksManager.Blocks[Terrain.ExtractContents(blockValue)], 0, blockValue)).OfType<IRecipaediaItem>().ToArray<ArraySelect<int, BlockItem>, BlockItem, IRecipaediaItem>(), m_navigator);
			m_ingredientWidget.CountTextProcesser = (int _) => cokingRecipe.IngredientCount.ToString();
			m_resultWidget.SetResult(new BlockItem(BlocksManager.Blocks[Terrain.ExtractContents(cokingRecipe.ResultID)], 0, cokingRecipe.ResultID), m_navigator, cokingRecipe.ResultCount);
			m_fireWidget.ParticlesPerSecond = 40f;
		}
	}

	public override void Hide()
	{
		m_nameWidget.IsVisible = false;
		m_descriptionWidget.IsVisible = false;
		m_ingredientWidget.ClearContents();
		m_resultWidget.ClearContents();
		m_fireWidget.ParticlesPerSecond = 0f;
	}

	public override CrafterButtonWidget GetCrafterButton(IRecipe recipe)
	{
		return CrafterButtonWidget.GetDefaultWidget(recipe, m_navigator);
	}
}
