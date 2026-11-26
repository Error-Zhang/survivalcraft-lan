using System;
using System.Xml.Linq;
using Engine;
using Game;
using RecipaediaEX;
using RecipaediaEX.Implementation;
using RecipaediaEX.UI;
using SCIENEW.RecipaediaEX.Recipes;
using SCIENEW.Utils;
using ZLinq;
using ZLinq.Linq;

namespace SCIENEW.Widgets.Recipes.Descriptors;

[RecipeDescriptor(new Type[] { typeof(IECraftingRecipe) }, 0)]
public class IECraftingRecipeDescriptor : RecipeDescriptor
{
	public LabelWidget m_nameWidget;

	public LabelWidget m_descriptionWidget;

	public GridPanelWidget m_gridWidget;

	public BlockRecipeSlotWidget m_resultWidget;

	public IECraftingRecipeDescriptor(IRecipaediaRecipeNavigator navigator)
		: base(navigator)
	{
		XElement node = ContentManager.Get<XElement>("Widgets/RecipeDescriptors/IECraftingRecipeDescriptor");
		LoadContents(this, node);
		m_nameWidget = Children.Find<LabelWidget>("IECraftingRecipeDescriptor.Name");
		m_descriptionWidget = Children.Find<LabelWidget>("IECraftingRecipeDescriptor.Description");
		m_gridWidget = Children.Find<GridPanelWidget>("IECraftingRecipeDescriptor.Ingredients");
		m_resultWidget = Children.Find<BlockRecipeSlotWidget>("IECraftingRecipeDescriptor.Result");
		for (int i = 0; i < m_gridWidget.RowsCount; i++)
		{
			for (int j = 0; j < m_gridWidget.ColumnsCount; j++)
			{
				BlockRecipeSlotWidget widget = new BlockRecipeSlotWidget
				{
					Size = new Vector2(48f, 48f)
				};
				m_gridWidget.Children.Add(widget);
				m_gridWidget.SetWidgetCell(widget, new Point2(j, i));
			}
		}
	}

	public override void Show(IRecipe recipe, string nameSuffix)
	{
		if (!(recipe is IECraftingRecipe iECraftingRecipe))
		{
			return;
		}
		Block block = BlocksManager.Blocks[Terrain.ExtractContents(iECraftingRecipe.ResultValue)];
		m_nameWidget.Text = block.GetDisplayName(null, iECraftingRecipe.ResultValue) + ((!string.IsNullOrEmpty(nameSuffix)) ? nameSuffix : string.Empty);
		m_descriptionWidget.Text = iECraftingRecipe.Description;
		m_nameWidget.IsVisible = true;
		m_descriptionWidget.IsVisible = true;
		foreach (BlockRecipeSlotWidget child in m_gridWidget.Children)
		{
			Point2 widgetCell = m_gridWidget.GetWidgetCell(child);
			string text = iECraftingRecipe.Ingredients[widgetCell.X + widgetCell.Y * 6];
			if (!string.IsNullOrEmpty(text))
			{
				CraftingRecipesManager.DecodeIngredient(text, out var craftingId, out var data);
				ValueEnumerable<FromArray<int>, int> source = RecipesUtils.FindBlocksByCraftingId(craftingId, data).AsValueEnumerable();
				child.SetIngredients(source.Select((int blockValue) => new BlockItem(BlocksManager.Blocks[Terrain.ExtractContents(blockValue)], 0, blockValue)).OfType<IRecipaediaItem>().ToArray<ArraySelect<int, BlockItem>, BlockItem, IRecipaediaItem>(), m_navigator);
			}
		}
		m_resultWidget.SetResult(new BlockItem(BlocksManager.Blocks[Terrain.ExtractContents(iECraftingRecipe.ResultValue)], 0, iECraftingRecipe.ResultValue), m_navigator, iECraftingRecipe.ResultCount);
	}

	public override void Hide()
	{
		m_nameWidget.IsVisible = false;
		m_descriptionWidget.IsVisible = false;
		foreach (BlockRecipeSlotWidget child in m_gridWidget.Children)
		{
			child.ClearContents();
		}
		m_resultWidget.ClearContents();
	}

	public override CrafterButtonWidget GetCrafterButton(IRecipe recipe)
	{
		return CrafterButtonWidget.GetDefaultWidget(recipe, m_navigator);
	}
}
