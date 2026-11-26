using System;
using System.Xml.Linq;
using Game;
using RecipaediaEX;
using RecipaediaEX.Implementation;
using RecipaediaEX.UI;
using SCIENEW.RecipaediaEX.Items;
using SCIENEW.RecipaediaEX.Recipes;
using SCIENEW.Widgets.Recipes.Slots;
using ZLinq;
using ZLinq.Linq;

namespace SCIENEW.Widgets.Recipes.Descriptors;

[RecipeDescriptor(new Type[] { typeof(CastingEquationRecipe) }, 0)]
public class CastingEquationRecipeDescriptor : RecipeDescriptor
{
	public LabelWidget m_nameWidget;

	public LabelWidget m_descriptionWidget;

	public FluidRecipeSlotWidget m_ingredientWidget;

	public BlockRecipeSlotWidget m_resultWidget;

	public BlockRecipeSlotWidget m_moduleWidget;

	public CastingEquationRecipeDescriptor(IRecipaediaRecipeNavigator navigator)
		: base(navigator)
	{
		XElement node = ContentManager.Get<XElement>("Widgets/RecipeDescriptors/CastingEquationRecipeDescriptor");
		LoadContents(this, node);
		m_nameWidget = Children.Find<LabelWidget>("CastingEquationRecipeDescriptor.Name");
		m_descriptionWidget = Children.Find<LabelWidget>("CastingEquationRecipeDescriptor.Description");
		m_ingredientWidget = Children.Find<FluidRecipeSlotWidget>("CastingEquationRecipeDescriptor.Ingredient");
		m_resultWidget = Children.Find<BlockRecipeSlotWidget>("CastingEquationRecipeDescriptor.Result");
		m_moduleWidget = Children.Find<BlockRecipeSlotWidget>("CastingEquationRecipeDescriptor.Module");
	}

	public override void Show(IRecipe recipe, string nameSuffix)
	{
		if (!(recipe is CastingEquationRecipe castingEquationRecipe))
		{
			return;
		}
		Block block = BlocksManager.Blocks[Terrain.ExtractContents(castingEquationRecipe.ResultValue)];
		m_nameWidget.Text = block.GetDisplayName(null, castingEquationRecipe.ResultValue) + ((!string.IsNullOrEmpty(nameSuffix)) ? nameSuffix : string.Empty);
		m_descriptionWidget.Text = castingEquationRecipe.Description;
		m_nameWidget.IsVisible = true;
		m_descriptionWidget.IsVisible = true;
		m_ingredientWidget.SetIngredients(new IRecipaediaItem[1]
		{
			new FluidItem(castingEquationRecipe.IngredientFluidValue, FluidsManager.GetFluid(castingEquationRecipe.IngredientFluidValue))
		}, m_navigator);
		m_ingredientWidget.m_volume = castingEquationRecipe.IngredientFluidVolume;
		m_resultWidget.SetResult(new BlockItem(block, 0, castingEquationRecipe.ResultValue), m_navigator, castingEquationRecipe.ResultCount);
		m_resultWidget.IsCountVisible = BlockRecipeSlotWidget.DefaultIsCountVisible;
		m_resultWidget.CountTextProcesser = BlockRecipeSlotWidget.DefaultCountTextProcesser;
		if (castingEquationRecipe.ModuleValues.Length == 0 || castingEquationRecipe.ModuleCount <= 0)
		{
			m_moduleWidget.ClearContents();
			m_moduleWidget.IsCountVisible = BlockRecipeSlotWidget.DefaultIsCountVisible;
			m_moduleWidget.CountTextProcesser = BlockRecipeSlotWidget.DefaultCountTextProcesser;
			return;
		}
		int moduleCount = castingEquationRecipe.ModuleCount;
		m_moduleWidget.SetIngredients((from v in castingEquationRecipe.ModuleValues.AsValueEnumerable()
			select new BlockItem(BlocksManager.Blocks[Terrain.ExtractContents(v)], 0, v)).OfType<IRecipaediaItem>().ToArray<ArraySelect<int, BlockItem>, BlockItem, IRecipaediaItem>(), m_navigator);
		m_moduleWidget.IsCountVisible = (BlockRecipeSlotWidget _) => true;
		m_moduleWidget.CountTextProcesser = (int _) => moduleCount.ToString();
	}

	public override void Hide()
	{
		m_nameWidget.IsVisible = false;
		m_descriptionWidget.IsVisible = false;
		m_ingredientWidget.ClearContents();
		m_ingredientWidget.m_volume = 0f;
		m_resultWidget.ClearContents();
		m_resultWidget.IsCountVisible = BlockRecipeSlotWidget.DefaultIsCountVisible;
		m_resultWidget.CountTextProcesser = BlockRecipeSlotWidget.DefaultCountTextProcesser;
		m_moduleWidget.ClearContents();
		m_moduleWidget.IsCountVisible = BlockRecipeSlotWidget.DefaultIsCountVisible;
		m_moduleWidget.CountTextProcesser = BlockRecipeSlotWidget.DefaultCountTextProcesser;
	}

	public override CrafterButtonWidget GetCrafterButton(IRecipe recipe)
	{
		return CrafterButtonWidget.GetDefaultWidget(recipe, m_navigator);
	}
}
