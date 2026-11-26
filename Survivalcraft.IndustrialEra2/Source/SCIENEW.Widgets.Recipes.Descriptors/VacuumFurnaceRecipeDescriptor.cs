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

[RecipeDescriptor(new Type[] { typeof(VacuumFurnaceRecipe) }, 0)]
public class VacuumFurnaceRecipeDescriptor : RecipeDescriptor
{
	public LabelWidget m_nameWidget;

	public LabelWidget m_descriptionWidget;

	public BlockRecipeSlotWidget m_ingredient1Widget;

	public BlockRecipeSlotWidget m_ingredient2Widget;

	public BlockRecipeSlotWidget m_ingredient3Widget;

	public BlockRecipeSlotWidget m_ingredient4Widget;

	public BlockRecipeSlotWidget m_resultWidget;

	public FireWidget m_fireWidget;

	public VacuumFurnaceRecipeDescriptor(IRecipaediaRecipeNavigator navigator)
		: base(navigator)
	{
		XElement node = ContentManager.Get<XElement>("Widgets/RecipeDescriptors/VacuumFurnaceRecipeDescriptor");
		LoadContents(this, node);
		m_nameWidget = Children.Find<LabelWidget>("VacuumFurnaceRecipeDescriptor.Name");
		m_descriptionWidget = Children.Find<LabelWidget>("VacuumFurnaceRecipeDescriptor.Description");
		m_ingredient1Widget = Children.Find<BlockRecipeSlotWidget>("VacuumFurnaceRecipeDescriptor.Ingredient1");
		m_ingredient2Widget = Children.Find<BlockRecipeSlotWidget>("VacuumFurnaceRecipeDescriptor.Ingredient2");
		m_ingredient3Widget = Children.Find<BlockRecipeSlotWidget>("VacuumFurnaceRecipeDescriptor.Ingredient3");
		m_ingredient4Widget = Children.Find<BlockRecipeSlotWidget>("VacuumFurnaceRecipeDescriptor.Ingredient4");
		m_resultWidget = Children.Find<BlockRecipeSlotWidget>("VacuumFurnaceRecipeDescriptor.Result");
		m_fireWidget = Children.Find<FireWidget>("VacuumFurnaceRecipeDescriptor.Fire");
	}

	public override void Show(IRecipe recipe, string nameSuffix)
	{
		if (recipe is VacuumFurnaceRecipe vacuumFurnaceRecipe)
		{
			Block block = BlocksManager.Blocks[Terrain.ExtractContents(vacuumFurnaceRecipe.ResultID)];
			m_nameWidget.Text = block.GetDisplayName(null, vacuumFurnaceRecipe.ResultID) + ((!string.IsNullOrEmpty(nameSuffix)) ? nameSuffix : string.Empty);
			m_descriptionWidget.Text = vacuumFurnaceRecipe.Description;
			m_nameWidget.IsVisible = true;
			m_descriptionWidget.IsVisible = true;
			SetIngredientSlotOrClear(m_ingredient1Widget, vacuumFurnaceRecipe.Ingredient1);
			SetIngredientSlotOrClear(m_ingredient2Widget, vacuumFurnaceRecipe.Ingredient2);
			SetIngredientSlotOrClear(m_ingredient3Widget, vacuumFurnaceRecipe.Ingredient3);
			SetIngredientSlotOrClear(m_ingredient4Widget, vacuumFurnaceRecipe.Ingredient4);
			m_resultWidget.SetResult(new BlockItem(block, 0, vacuumFurnaceRecipe.ResultID), m_navigator, vacuumFurnaceRecipe.ResultCount);
			m_fireWidget.ParticlesPerSecond = 24f;
		}
	}

	public override void Hide()
	{
		m_nameWidget.IsVisible = false;
		m_descriptionWidget.IsVisible = false;
		m_ingredient1Widget.ClearContents();
		m_ingredient2Widget.ClearContents();
		m_ingredient3Widget.ClearContents();
		m_ingredient4Widget.ClearContents();
		m_resultWidget.ClearContents();
		m_fireWidget.ParticlesPerSecond = 0f;
	}

	public override CrafterButtonWidget GetCrafterButton(IRecipe recipe)
	{
		return CrafterButtonWidget.GetDefaultWidget(recipe, m_navigator);
	}

	private void SetIngredientSlotOrClear(BlockRecipeSlotWidget slot, string ingredient)
	{
		if (string.IsNullOrEmpty(ingredient))
		{
			slot.ClearContents();
		}
		else
		{
			SetIngredientSlot(slot, ingredient);
		}
	}

	private void SetIngredientSlot(BlockRecipeSlotWidget slot, string ingredient)
	{
		CraftingRecipesManager.DecodeIngredient(ingredient, out var craftingId, out var data);
		ValueEnumerable<FromArray<int>, int> source = RecipesUtils.FindBlocksByCraftingId(craftingId, data).AsValueEnumerable();
		slot.SetIngredients(source.Select((int blockValue) => new BlockItem(BlocksManager.Blocks[Terrain.ExtractContents(blockValue)], 0, blockValue)).OfType<IRecipaediaItem>().ToArray<ArraySelect<int, BlockItem>, BlockItem, IRecipaediaItem>(), m_navigator);
	}
}
