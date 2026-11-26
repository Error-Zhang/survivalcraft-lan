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

[RecipeDescriptor(new Type[] { typeof(SiftingRecipe) }, 0)]
public class SiftingRecipeDescriptor : RecipeDescriptor
{
	public LabelWidget m_nameWidget;

	public LabelWidget m_descriptionWidget;

	public GridPanelWidget m_resultGridWidget;

	public BlockRecipeSlotWidget m_ingredientWidget;

	public SiftingRecipeDescriptor(IRecipaediaRecipeNavigator navigator)
		: base(navigator)
	{
		XElement node = ContentManager.Get<XElement>("Widgets/RecipeDescriptors/SiftingRecipeDescriptor");
		LoadContents(this, node);
		m_nameWidget = Children.Find<LabelWidget>("SiftingRecipeDescriptor.Name");
		m_descriptionWidget = Children.Find<LabelWidget>("SiftingRecipeDescriptor.Description");
		m_resultGridWidget = Children.Find<GridPanelWidget>("SiftingRecipeDescriptor.Results");
		m_ingredientWidget = Children.Find<BlockRecipeSlotWidget>("SiftingRecipeDescriptor.Ingredient");
		for (int i = 0; i < m_resultGridWidget.RowsCount; i++)
		{
			for (int j = 0; j < m_resultGridWidget.ColumnsCount; j++)
			{
				BlockRecipeSlotWidget widget = new BlockRecipeSlotWidget
				{
					IsCountVisible = (BlockRecipeSlotWidget _) => true,
					CountTextProcesser = null,
					m_countLabel =
					{
						FontScale = 0.75f
					}
				};
				m_resultGridWidget.Children.Add(widget);
				m_resultGridWidget.SetWidgetCell(widget, new Point2(j, i));
			}
		}
	}

	public override void Show(IRecipe recipe, string nameSuffix)
	{
		if (!(recipe is SiftingRecipe siftingRecipe))
		{
			return;
		}
		ValueEnumerable<FromArray<SiftingRecipe.SiftingResult>, SiftingRecipe.SiftingResult> source = siftingRecipe.Results.AsValueEnumerable();
		var source2 = from x in source
			select x.Value into x
			select new
			{
				Block = BlocksManager.Blocks[Terrain.ExtractContents(x)],
				Value = x
			};
		var source3 = source2.Select(info => info.Block.GetDisplayName(null, info.Value));
		m_nameWidget.Text = string.Join(" ", source3.ToArray()) + ((!string.IsNullOrEmpty(nameSuffix)) ? nameSuffix : string.Empty);
		m_descriptionWidget.Text = siftingRecipe.Description;
		m_nameWidget.IsVisible = true;
		m_descriptionWidget.IsVisible = true;
		CraftingRecipesManager.DecodeIngredient(siftingRecipe.Ingredient, out var craftingId, out var data);
		ValueEnumerable<FromArray<int>, int> source4 = RecipesUtils.FindBlocksByCraftingId(craftingId, data).AsValueEnumerable();
		m_ingredientWidget.SetIngredients(source4.Select((int blockValue) => new BlockItem(BlocksManager.Blocks[Terrain.ExtractContents(blockValue)], 0, blockValue)).OfType<IRecipaediaItem>().ToArray<ArraySelect<int, BlockItem>, BlockItem, IRecipaediaItem>(), m_navigator);
		int num = 0;
		float num2 = source.Sum((SiftingRecipe.SiftingResult x) => x.Weight);
		ValueEnumerator<OfType<FromEnumerable<Widget>, Widget, BlockRecipeSlotWidget>, BlockRecipeSlotWidget> enumerator = ValueEnumerableExtensions.GetEnumerator<OfType<FromEnumerable<Widget>, Widget, BlockRecipeSlotWidget>, BlockRecipeSlotWidget>(in ILSpyHelper_AsRefReadOnly(m_resultGridWidget.Children.AsValueEnumerable().OfType<BlockRecipeSlotWidget>()));
		try
		{
			while (enumerator.MoveNext())
			{
				BlockRecipeSlotWidget current = enumerator.Current;
				if (num >= siftingRecipe.Results.Length)
				{
					current.ClearContents();
					continue;
				}
				current.SetResult(new BlockItem(source2.ElementAt(num).Block, 0, source2.ElementAt(num).Value), m_navigator, source.ElementAt(num).Count);
				current.m_countLabel.Text = $"{source.ElementAt(num).Weight / num2:P0}({source.ElementAt(num).Count})";
				num++;
			}
		}
		finally
		{
			enumerator.Dispose();
		}
		static ref readonly T ILSpyHelper_AsRefReadOnly<T>(in T temp) where T : allows ref struct
		{
			//ILSpy generated this function to help ensure overload resolution can pick the overload using 'in'
			return ref temp;
		}
	}

	public override void Hide()
	{
		m_nameWidget.IsVisible = false;
		m_descriptionWidget.IsVisible = false;
		ValueEnumerator<OfType<FromEnumerable<Widget>, Widget, BlockRecipeSlotWidget>, BlockRecipeSlotWidget> enumerator = ValueEnumerableExtensions.GetEnumerator<OfType<FromEnumerable<Widget>, Widget, BlockRecipeSlotWidget>, BlockRecipeSlotWidget>(in ILSpyHelper_AsRefReadOnly(m_resultGridWidget.Children.AsValueEnumerable().OfType<BlockRecipeSlotWidget>()));
		try
		{
			while (enumerator.MoveNext())
			{
				enumerator.Current.ClearContents();
			}
		}
		finally
		{
			enumerator.Dispose();
		}
		m_ingredientWidget.ClearContents();
		static ref readonly T ILSpyHelper_AsRefReadOnly<T>(in T temp) where T : allows ref struct
		{
			//ILSpy generated this function to help ensure overload resolution can pick the overload using 'in'
			return ref temp;
		}
	}

	public override CrafterButtonWidget GetCrafterButton(IRecipe recipe)
	{
		return CrafterButtonWidget.GetDefaultWidget(recipe, m_navigator);
	}
}
