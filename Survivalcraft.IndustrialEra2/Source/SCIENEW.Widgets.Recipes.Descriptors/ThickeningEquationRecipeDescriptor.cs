using System;
using System.Xml.Linq;
using Game;
using RecipaediaEX;
using RecipaediaEX.Implementation;
using RecipaediaEX.UI;
using SCIENEW.RecipaediaEX.Items;
using SCIENEW.RecipaediaEX.Recipes;
using SCIENEW.Widgets.Recipes.Slots;

namespace SCIENEW.Widgets.Recipes.Descriptors;

[RecipeDescriptor(new Type[] { typeof(ThickeningEquationRecipe) }, 0)]
public class ThickeningEquationRecipeDescriptor : RecipeDescriptor
{
	public LabelWidget m_nameWidget;

	public LabelWidget m_descriptionWidget;

	public FluidRecipeSlotWidget m_fluidWidget;

	public BlockRecipeSlotWidget m_rawWidget;

	public BlockRecipeSlotWidget m_resultWidget;

	public ThickeningEquationRecipeDescriptor(IRecipaediaRecipeNavigator navigator)
		: base(navigator)
	{
		XElement node = ContentManager.Get<XElement>("Widgets/RecipeDescriptors/ThickeningEquationRecipeDescriptor");
		LoadContents(this, node);
		m_nameWidget = Children.Find<LabelWidget>("ThickeningEquationRecipeDescriptor.Name");
		m_descriptionWidget = Children.Find<LabelWidget>("ThickeningEquationRecipeDescriptor.Description");
		m_fluidWidget = Children.Find<FluidRecipeSlotWidget>("ThickeningEquationRecipeDescriptor.Fluid");
		m_rawWidget = Children.Find<BlockRecipeSlotWidget>("ThickeningEquationRecipeDescriptor.Raw");
		m_resultWidget = Children.Find<BlockRecipeSlotWidget>("ThickeningEquationRecipeDescriptor.Result");
	}

	public override void Show(IRecipe recipe, string nameSuffix)
	{
		if (recipe is ThickeningEquationRecipe thickeningEquationRecipe)
		{
			Block block = BlocksManager.Blocks[Terrain.ExtractContents(thickeningEquationRecipe.ResultValue)];
			m_nameWidget.Text = block.GetDisplayName(null, thickeningEquationRecipe.ResultValue) + ((!string.IsNullOrEmpty(nameSuffix)) ? nameSuffix : string.Empty);
			m_descriptionWidget.Text = thickeningEquationRecipe.Description;
			m_nameWidget.IsVisible = true;
			m_descriptionWidget.IsVisible = true;
			m_fluidWidget.SetIngredients(new IRecipaediaItem[1]
			{
				new FluidItem(thickeningEquationRecipe.IngredientFluidValue, FluidsManager.GetFluid(thickeningEquationRecipe.IngredientFluidValue))
			}, m_navigator);
			m_fluidWidget.m_volume = thickeningEquationRecipe.IngredientFluidVolume;
			Block block2 = BlocksManager.Blocks[Terrain.ExtractContents(thickeningEquationRecipe.IngredientValue)];
			m_rawWidget.SetIngredients(new IRecipaediaItem[1]
			{
				new BlockItem(block2, 0, thickeningEquationRecipe.IngredientValue)
			}, m_navigator);
			m_rawWidget.IsCountVisible = BlockRecipeSlotWidget.DefaultIsCountVisible;
			m_rawWidget.CountTextProcesser = BlockRecipeSlotWidget.DefaultCountTextProcesser;
			m_resultWidget.SetResult(new BlockItem(block, 0, thickeningEquationRecipe.ResultValue), m_navigator, thickeningEquationRecipe.ResultCount);
			m_resultWidget.IsCountVisible = BlockRecipeSlotWidget.DefaultIsCountVisible;
			m_resultWidget.CountTextProcesser = BlockRecipeSlotWidget.DefaultCountTextProcesser;
		}
	}

	public override void Hide()
	{
		m_nameWidget.IsVisible = false;
		m_descriptionWidget.IsVisible = false;
		m_fluidWidget.ClearContents();
		m_fluidWidget.m_volume = 0f;
		m_rawWidget.ClearContents();
		m_rawWidget.IsCountVisible = BlockRecipeSlotWidget.DefaultIsCountVisible;
		m_rawWidget.CountTextProcesser = BlockRecipeSlotWidget.DefaultCountTextProcesser;
		m_resultWidget.ClearContents();
		m_resultWidget.IsCountVisible = BlockRecipeSlotWidget.DefaultIsCountVisible;
		m_resultWidget.CountTextProcesser = BlockRecipeSlotWidget.DefaultCountTextProcesser;
	}

	public override CrafterButtonWidget GetCrafterButton(IRecipe recipe)
	{
		return CrafterButtonWidget.GetDefaultWidget(recipe, m_navigator);
	}
}
