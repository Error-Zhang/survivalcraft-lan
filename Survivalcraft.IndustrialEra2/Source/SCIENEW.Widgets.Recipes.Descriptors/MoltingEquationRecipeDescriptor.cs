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

[RecipeDescriptor(new Type[] { typeof(MoltingEquationRecipe) }, 0)]
public class MoltingEquationRecipeDescriptor : RecipeDescriptor
{
	public LabelWidget m_nameWidget;

	public LabelWidget m_descriptionWidget;

	public BlockRecipeSlotWidget m_ingredientWidget;

	public FluidRecipeSlotWidget m_resultWidget;

	public FireWidget m_fireWidget;

	public MoltingEquationRecipeDescriptor(IRecipaediaRecipeNavigator navigator)
		: base(navigator)
	{
		XElement node = ContentManager.Get<XElement>("Widgets/RecipeDescriptors/MoltingEquationRecipeDescriptor");
		LoadContents(this, node);
		m_nameWidget = Children.Find<LabelWidget>("MoltingEquationRecipeDescriptor.Name");
		m_descriptionWidget = Children.Find<LabelWidget>("MoltingEquationRecipeDescriptor.Description");
		m_ingredientWidget = Children.Find<BlockRecipeSlotWidget>("MoltingEquationRecipeDescriptor.Ingredient");
		m_resultWidget = Children.Find<FluidRecipeSlotWidget>("MoltingEquationRecipeDescriptor.Result");
		m_fireWidget = Children.Find<FireWidget>("MoltingEquationRecipeDescriptor.Fire");
	}

	public override void Show(IRecipe recipe, string nameSuffix)
	{
		MoltingEquationRecipe moltingRecipe = recipe as MoltingEquationRecipe;
		if (moltingRecipe != null)
		{
			Fluid fluid = FluidsManager.GetFluid(moltingRecipe.ResultFluidValue);
			m_nameWidget.Text = fluid.GetDisplayName(moltingRecipe.ResultFluidValue) + ((!string.IsNullOrEmpty(nameSuffix)) ? nameSuffix : string.Empty);
			m_descriptionWidget.Text = moltingRecipe.Description;
			m_nameWidget.IsVisible = true;
			m_descriptionWidget.IsVisible = true;
			m_ingredientWidget.SetIngredients(new IRecipaediaItem[1]
			{
				new BlockItem(BlocksManager.Blocks[Terrain.ExtractContents(moltingRecipe.IngredientValue)], 0, moltingRecipe.IngredientValue)
			}, m_navigator);
			m_ingredientWidget.IsCountVisible = (BlockRecipeSlotWidget _) => true;
			m_ingredientWidget.CountTextProcesser = (int _) => moltingRecipe.IngredientCount.ToString();
			m_resultWidget.SetResult(new FluidItem(moltingRecipe.ResultFluidValue, fluid), m_navigator, moltingRecipe.ResultFluidVolume);
			m_fireWidget.ParticlesPerSecond = 24f;
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
		m_fireWidget.ParticlesPerSecond = 0f;
	}

	public override CrafterButtonWidget GetCrafterButton(IRecipe recipe)
	{
		return CrafterButtonWidget.GetDefaultWidget(recipe, m_navigator);
	}
}
