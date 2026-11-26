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

[RecipeDescriptor(new Type[] { typeof(FermentingEquationRecipe) }, 0)]
public class FermentingEquationRecipeDescriptor : RecipeDescriptor
{
	public LabelWidget m_nameWidget;

	public LabelWidget m_descriptionWidget;

	public BlockRecipeSlotWidget m_ingredientWidget;

	public FluidRecipeSlotWidget m_resultWidget;

	public FermentingEquationRecipeDescriptor(IRecipaediaRecipeNavigator navigator)
		: base(navigator)
	{
		XElement node = ContentManager.Get<XElement>("Widgets/RecipeDescriptors/FermentingEquationRecipeDescriptor");
		LoadContents(this, node);
		m_nameWidget = Children.Find<LabelWidget>("FermentingEquationRecipeDescriptor.Name");
		m_descriptionWidget = Children.Find<LabelWidget>("FermentingEquationRecipeDescriptor.Description");
		m_ingredientWidget = Children.Find<BlockRecipeSlotWidget>("FermentingEquationRecipeDescriptor.Ingredient");
		m_resultWidget = Children.Find<FluidRecipeSlotWidget>("FermentingEquationRecipeDescriptor.Result");
	}

	public override void Show(IRecipe recipe, string nameSuffix)
	{
		FermentingEquationRecipe fermentingRecipe = recipe as FermentingEquationRecipe;
		if (fermentingRecipe != null)
		{
			Fluid fluid = FluidsManager.GetFluid(fermentingRecipe.ResultFluidValue);
			m_nameWidget.Text = fluid.GetDisplayName(fermentingRecipe.ResultFluidValue) + ((!string.IsNullOrEmpty(nameSuffix)) ? nameSuffix : string.Empty);
			m_descriptionWidget.Text = fermentingRecipe.Description;
			m_nameWidget.IsVisible = true;
			m_descriptionWidget.IsVisible = true;
			m_ingredientWidget.SetIngredients(new IRecipaediaItem[1]
			{
				new BlockItem(BlocksManager.Blocks[Terrain.ExtractContents(fermentingRecipe.IngredientValue)], 0, fermentingRecipe.IngredientValue)
			}, m_navigator);
			m_ingredientWidget.IsCountVisible = (BlockRecipeSlotWidget _) => true;
			m_ingredientWidget.CountTextProcesser = (int _) => fermentingRecipe.IngredientCount.ToString();
			m_resultWidget.SetResult(new FluidItem(fermentingRecipe.ResultFluidValue, fluid), m_navigator, fermentingRecipe.ResultFluidVolume);
		}
	}

	public override void Hide()
	{
		m_nameWidget.IsVisible = false;
		m_descriptionWidget.IsVisible = false;
		m_ingredientWidget.ClearContents();
		m_ingredientWidget.IsCountVisible = BlockRecipeSlotWidget.DefaultIsCountVisible;
		m_ingredientWidget.CountTextProcesser = BlockRecipeSlotWidget.DefaultCountTextProcesser;
		m_resultWidget.ClearContents();
		m_resultWidget.m_volume = 0f;
	}

	public override CrafterButtonWidget GetCrafterButton(IRecipe recipe)
	{
		return CrafterButtonWidget.GetDefaultWidget(recipe, m_navigator);
	}
}
