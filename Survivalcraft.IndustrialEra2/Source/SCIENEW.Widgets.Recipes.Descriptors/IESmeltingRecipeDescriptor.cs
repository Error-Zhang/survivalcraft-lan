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

[RecipeDescriptor(new Type[] { typeof(IESmeltingRecipe) }, 0)]
public class IESmeltingRecipeDescriptor : RecipeDescriptor
{
	public LabelWidget m_nameWidget;

	public LabelWidget m_descriptionWidget;

	public GridPanelWidget m_gridWidget;

	public FireWidget m_fireWidget;

	public BlockRecipeSlotWidget m_resultWidget;

	public ValueBarWidget m_temperatureBar;

	public IESmeltingRecipe? m_recipe;

	public IESmeltingRecipeDescriptor(IRecipaediaRecipeNavigator navigator)
		: base(navigator)
	{
		XElement node = ContentManager.Get<XElement>("Widgets/RecipeDescriptors/IESmeltingRecipeDescriptor");
		LoadContents(this, node);
		m_nameWidget = Children.Find<LabelWidget>("IESmeltingRecipeDescriptor.Name");
		m_descriptionWidget = Children.Find<LabelWidget>("IESmeltingRecipeDescriptor.Description");
		m_gridWidget = Children.Find<GridPanelWidget>("IESmeltingRecipeDescriptor.Ingredients");
		m_fireWidget = Children.Find<FireWidget>("IESmeltingRecipeDescriptor.Fire");
		m_resultWidget = Children.Find<BlockRecipeSlotWidget>("IESmeltingRecipeDescriptor.Result");
		m_temperatureBar = Children.Find<ValueBarWidget>("IESmeltingRecipeDescriptor.TemperatureBar");
		for (int i = 0; i < m_gridWidget.RowsCount; i++)
		{
			for (int j = 0; j < m_gridWidget.ColumnsCount; j++)
			{
				BlockRecipeSlotWidget widget = new BlockRecipeSlotWidget();
				m_gridWidget.Children.Add(widget);
				m_gridWidget.SetWidgetCell(widget, new Point2(j, i));
			}
		}
	}

	public override void Show(IRecipe recipe, string nameSuffix)
	{
		if (!(recipe is IESmeltingRecipe iESmeltingRecipe))
		{
			return;
		}
		m_recipe = iESmeltingRecipe;
		Block block = BlocksManager.Blocks[Terrain.ExtractContents(iESmeltingRecipe.ResultValue)];
		m_nameWidget.Text = block.GetDisplayName(null, iESmeltingRecipe.ResultValue) + ((!string.IsNullOrEmpty(nameSuffix)) ? nameSuffix : string.Empty);
		m_descriptionWidget.Text = iESmeltingRecipe.Description;
		m_nameWidget.IsVisible = true;
		m_descriptionWidget.IsVisible = true;
		foreach (BlockRecipeSlotWidget child in m_gridWidget.Children)
		{
			Point2 widgetCell = m_gridWidget.GetWidgetCell(child);
			string text = iESmeltingRecipe.Ingredients[widgetCell.X + widgetCell.Y * 6];
			if (!string.IsNullOrEmpty(text))
			{
				CraftingRecipesManager.DecodeIngredient(text, out var craftingId, out var data);
				ValueEnumerable<FromArray<int>, int> source = RecipesUtils.FindBlocksByCraftingId(craftingId, data).AsValueEnumerable();
				child.SetIngredients(source.Select((int blockValue) => new BlockItem(BlocksManager.Blocks[Terrain.ExtractContents(blockValue)], 0, blockValue)).OfType<IRecipaediaItem>().ToArray<ArraySelect<int, BlockItem>, BlockItem, IRecipaediaItem>(), m_navigator);
			}
		}
		m_resultWidget.SetResult(new BlockItem(BlocksManager.Blocks[Terrain.ExtractContents(iESmeltingRecipe.ResultValue)], 0, iESmeltingRecipe.ResultValue), m_navigator, iESmeltingRecipe.ResultCount);
		m_fireWidget.ParticlesPerSecond = 40f;
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
		m_fireWidget.ParticlesPerSecond = 0f;
	}

	public override void Update()
	{
		base.Update();
		if (m_recipe != null)
		{
			if (m_recipe.RequiredHeatLevel >= 7f)
			{
				m_temperatureBar.BarSubtexture = ContentManager.Get<Subtexture>("Textures/Atlas/Temperature6");
			}
			else if (m_recipe.RequiredHeatLevel >= 6f)
			{
				m_temperatureBar.BarSubtexture = ContentManager.Get<Subtexture>("Textures/Atlas/Temperature5");
			}
			else if (m_recipe.RequiredHeatLevel >= 5f)
			{
				m_temperatureBar.BarSubtexture = ContentManager.Get<Subtexture>("Textures/Atlas/Temperature4");
			}
			else if (m_recipe.RequiredHeatLevel >= 4f)
			{
				m_temperatureBar.BarSubtexture = ContentManager.Get<Subtexture>("Textures/Atlas/Temperature3");
			}
			else if (m_recipe.RequiredHeatLevel >= 3f)
			{
				m_temperatureBar.BarSubtexture = ContentManager.Get<Subtexture>("Textures/Atlas/Temperature2");
			}
			else
			{
				m_temperatureBar.BarSubtexture = ((m_recipe.RequiredHeatLevel >= 2f) ? ContentManager.Get<Subtexture>("Textures/Atlas/Temperature1") : ContentManager.Get<Subtexture>("Textures/Atlas/Temperature0"));
			}
		}
	}

	public override CrafterButtonWidget GetCrafterButton(IRecipe recipe)
	{
		return CrafterButtonWidget.GetDefaultWidget(recipe, m_navigator);
	}
}
