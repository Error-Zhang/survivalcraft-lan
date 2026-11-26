using System;
using System.Xml.Linq;
using Engine;
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

[RecipeDescriptor(new Type[] { typeof(PetrochemicalEquationRecipe) }, 0)]
public class PetrochemicalEquationRecipeDescriptor : RecipeDescriptor
{
	public LabelWidget m_nameWidget;

	public LabelWidget m_descriptionWidget;

	public GridPanelWidget m_materialsItemsWidget;

	public GridPanelWidget m_materialsFluidsWidget;

	public GridPanelWidget m_resultsItemsWidget;

	public GridPanelWidget m_resultsFluidsWidget;

	public PetrochemicalEquationRecipeDescriptor(IRecipaediaRecipeNavigator navigator)
		: base(navigator)
	{
		XElement node = ContentManager.Get<XElement>("Widgets/RecipeDescriptors/PetrochemicalEquationRecipeDescriptor");
		LoadContents(this, node);
		m_nameWidget = Children.Find<LabelWidget>("PetrochemicalEquationRecipeDescriptor.Name");
		m_descriptionWidget = Children.Find<LabelWidget>("PetrochemicalEquationRecipeDescriptor.Description");
		m_materialsItemsWidget = Children.Find<GridPanelWidget>("PetrochemicalEquationRecipeDescriptor.MaterialsItems");
		m_materialsFluidsWidget = Children.Find<GridPanelWidget>("PetrochemicalEquationRecipeDescriptor.MaterialsFluids");
		m_resultsItemsWidget = Children.Find<GridPanelWidget>("PetrochemicalEquationRecipeDescriptor.ResultsItems");
		m_resultsFluidsWidget = Children.Find<GridPanelWidget>("PetrochemicalEquationRecipeDescriptor.ResultsFluids");
		for (int i = 0; i < m_materialsItemsWidget.RowsCount; i++)
		{
			for (int j = 0; j < m_materialsItemsWidget.ColumnsCount; j++)
			{
				BlockRecipeSlotWidget widget = new BlockRecipeSlotWidget();
				m_materialsItemsWidget.Children.Add(widget);
				m_materialsItemsWidget.SetWidgetCell(widget, new Point2(j, i));
			}
		}
		for (int k = 0; k < m_materialsFluidsWidget.RowsCount; k++)
		{
			for (int l = 0; l < m_materialsFluidsWidget.ColumnsCount; l++)
			{
				FluidRecipeSlotWidget widget2 = new FluidRecipeSlotWidget();
				m_materialsFluidsWidget.Children.Add(widget2);
				m_materialsFluidsWidget.SetWidgetCell(widget2, new Point2(l, k));
			}
		}
		for (int m = 0; m < m_resultsItemsWidget.RowsCount; m++)
		{
			for (int n = 0; n < m_resultsItemsWidget.ColumnsCount; n++)
			{
				BlockRecipeSlotWidget widget3 = new BlockRecipeSlotWidget();
				m_resultsItemsWidget.Children.Add(widget3);
				m_resultsItemsWidget.SetWidgetCell(widget3, new Point2(n, m));
			}
		}
		for (int num = 0; num < m_resultsFluidsWidget.RowsCount; num++)
		{
			for (int num2 = 0; num2 < m_resultsFluidsWidget.ColumnsCount; num2++)
			{
				FluidRecipeSlotWidget widget4 = new FluidRecipeSlotWidget();
				m_resultsFluidsWidget.Children.Add(widget4);
				m_resultsFluidsWidget.SetWidgetCell(widget4, new Point2(num2, num));
			}
		}
	}

	public override void Show(IRecipe recipe, string nameSuffix)
	{
		if (!(recipe is PetrochemicalEquationRecipe petrochemicalEquationRecipe))
		{
			return;
		}
		string seed = petrochemicalEquationRecipe.ResultsFluids.AsValueEnumerable().Aggregate("", (string text, EquationFluid equationFluid3) => text + FluidsManager.GetFluid(equationFluid3.Value).GetDisplayName(equationFluid3.Value) + " ");
		seed = petrochemicalEquationRecipe.ResultsItems.AsValueEnumerable().Aggregate(seed, (string text, EquationItem equationItem) => text + BlocksManager.Blocks[Terrain.ExtractContents(equationItem.Values[0])].GetDisplayName(null, equationItem.Values[0]) + " ");
		m_nameWidget.Text = seed + ((!string.IsNullOrEmpty(nameSuffix)) ? nameSuffix : string.Empty);
		m_descriptionWidget.Text = petrochemicalEquationRecipe.Description;
		m_nameWidget.IsVisible = true;
		m_descriptionWidget.IsVisible = true;
		int num = 0;
		ValueEnumerator<OfType<FromEnumerable<Widget>, Widget, BlockRecipeSlotWidget>, BlockRecipeSlotWidget> enumerator = ValueEnumerableExtensions.GetEnumerator<OfType<FromEnumerable<Widget>, Widget, BlockRecipeSlotWidget>, BlockRecipeSlotWidget>(in ILSpyHelper_AsRefReadOnly(m_materialsItemsWidget.Children.AsValueEnumerable().OfType<BlockRecipeSlotWidget>()));
		try
		{
			while (enumerator.MoveNext())
			{
				BlockRecipeSlotWidget current = enumerator.Current;
				if (num >= petrochemicalEquationRecipe.MaterialsItems.Length || petrochemicalEquationRecipe.MaterialsItems[num].Values == null || petrochemicalEquationRecipe.MaterialsItems[num].Values.Length == 0)
				{
					current.ClearContents();
					current.IsCountVisible = BlockRecipeSlotWidget.DefaultIsCountVisible;
					current.CountTextProcesser = BlockRecipeSlotWidget.DefaultCountTextProcesser;
				}
				else
				{
					int ingredientCount = petrochemicalEquationRecipe.MaterialsItems[num].Count;
					current.SetIngredients((from v in petrochemicalEquationRecipe.MaterialsItems[num].Values.AsValueEnumerable()
						select new BlockItem(BlocksManager.Blocks[Terrain.ExtractContents(v)], 0, v)).OfType<IRecipaediaItem>().ToArray<ArraySelect<int, BlockItem>, BlockItem, IRecipaediaItem>(), m_navigator);
					current.IsCountVisible = (BlockRecipeSlotWidget _) => true;
					current.CountTextProcesser = (int _) => ingredientCount.ToString();
				}
				num++;
			}
		}
		finally
		{
			enumerator.Dispose();
		}
		num = 0;
		ValueEnumerator<OfType<FromEnumerable<Widget>, Widget, FluidRecipeSlotWidget>, FluidRecipeSlotWidget> enumerator2 = ValueEnumerableExtensions.GetEnumerator<OfType<FromEnumerable<Widget>, Widget, FluidRecipeSlotWidget>, FluidRecipeSlotWidget>(in ILSpyHelper_AsRefReadOnly(m_materialsFluidsWidget.Children.AsValueEnumerable().OfType<FluidRecipeSlotWidget>()));
		try
		{
			while (enumerator2.MoveNext())
			{
				FluidRecipeSlotWidget current2 = enumerator2.Current;
				if (num >= petrochemicalEquationRecipe.MaterialsFluids.Length)
				{
					current2.ClearContents();
					current2.m_volume = 0f;
				}
				else
				{
					EquationFluid equationFluid = petrochemicalEquationRecipe.MaterialsFluids[num];
					current2.SetIngredients(new IRecipaediaItem[1]
					{
						new FluidItem(equationFluid.Value, FluidsManager.GetFluid(equationFluid.Value))
					}, m_navigator);
					current2.m_volume = equationFluid.Volume;
				}
				num++;
			}
		}
		finally
		{
			enumerator2.Dispose();
		}
		num = 0;
		ValueEnumerator<OfType<FromEnumerable<Widget>, Widget, BlockRecipeSlotWidget>, BlockRecipeSlotWidget> enumerator3 = ValueEnumerableExtensions.GetEnumerator<OfType<FromEnumerable<Widget>, Widget, BlockRecipeSlotWidget>, BlockRecipeSlotWidget>(in ILSpyHelper_AsRefReadOnly(m_resultsItemsWidget.Children.AsValueEnumerable().OfType<BlockRecipeSlotWidget>()));
		try
		{
			while (enumerator3.MoveNext())
			{
				BlockRecipeSlotWidget current3 = enumerator3.Current;
				if (num >= petrochemicalEquationRecipe.ResultsItems.Length || petrochemicalEquationRecipe.ResultsItems[num].Values == null || petrochemicalEquationRecipe.ResultsItems[num].Values.Length == 0)
				{
					current3.ClearContents();
					current3.IsCountVisible = BlockRecipeSlotWidget.DefaultIsCountVisible;
					current3.CountTextProcesser = BlockRecipeSlotWidget.DefaultCountTextProcesser;
				}
				else
				{
					int num2 = petrochemicalEquationRecipe.ResultsItems[num].Values[0];
					current3.SetResult(new BlockItem(BlocksManager.Blocks[Terrain.ExtractContents(num2)], 0, num2), m_navigator, petrochemicalEquationRecipe.ResultsItems[num].Count);
					current3.IsCountVisible = BlockRecipeSlotWidget.DefaultIsCountVisible;
					current3.CountTextProcesser = BlockRecipeSlotWidget.DefaultCountTextProcesser;
				}
				num++;
			}
		}
		finally
		{
			enumerator3.Dispose();
		}
		num = 0;
		ValueEnumerator<OfType<FromEnumerable<Widget>, Widget, FluidRecipeSlotWidget>, FluidRecipeSlotWidget> enumerator4 = ValueEnumerableExtensions.GetEnumerator<OfType<FromEnumerable<Widget>, Widget, FluidRecipeSlotWidget>, FluidRecipeSlotWidget>(in ILSpyHelper_AsRefReadOnly(m_resultsFluidsWidget.Children.AsValueEnumerable().OfType<FluidRecipeSlotWidget>()));
		try
		{
			while (enumerator4.MoveNext())
			{
				FluidRecipeSlotWidget current4 = enumerator4.Current;
				if (num >= petrochemicalEquationRecipe.ResultsFluids.Length)
				{
					current4.ClearContents();
					current4.m_volume = 0f;
				}
				else
				{
					EquationFluid equationFluid2 = petrochemicalEquationRecipe.ResultsFluids[num];
					current4.SetResult(new FluidItem(equationFluid2.Value, FluidsManager.GetFluid(equationFluid2.Value)), m_navigator, equationFluid2.Volume);
				}
				num++;
			}
		}
		finally
		{
			enumerator4.Dispose();
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
		ValueEnumerator<OfType<FromEnumerable<Widget>, Widget, BlockRecipeSlotWidget>, BlockRecipeSlotWidget> enumerator = ValueEnumerableExtensions.GetEnumerator<OfType<FromEnumerable<Widget>, Widget, BlockRecipeSlotWidget>, BlockRecipeSlotWidget>(in ILSpyHelper_AsRefReadOnly(m_materialsItemsWidget.Children.AsValueEnumerable().OfType<BlockRecipeSlotWidget>()));
		try
		{
			while (enumerator.MoveNext())
			{
				BlockRecipeSlotWidget current = enumerator.Current;
				current.ClearContents();
				current.IsCountVisible = BlockRecipeSlotWidget.DefaultIsCountVisible;
				current.CountTextProcesser = BlockRecipeSlotWidget.DefaultCountTextProcesser;
			}
		}
		finally
		{
			enumerator.Dispose();
		}
		ValueEnumerator<OfType<FromEnumerable<Widget>, Widget, FluidRecipeSlotWidget>, FluidRecipeSlotWidget> enumerator2 = ValueEnumerableExtensions.GetEnumerator<OfType<FromEnumerable<Widget>, Widget, FluidRecipeSlotWidget>, FluidRecipeSlotWidget>(in ILSpyHelper_AsRefReadOnly(m_materialsFluidsWidget.Children.AsValueEnumerable().OfType<FluidRecipeSlotWidget>()));
		try
		{
			while (enumerator2.MoveNext())
			{
				FluidRecipeSlotWidget current2 = enumerator2.Current;
				current2.ClearContents();
				current2.m_volume = 0f;
			}
		}
		finally
		{
			enumerator2.Dispose();
		}
		ValueEnumerator<OfType<FromEnumerable<Widget>, Widget, BlockRecipeSlotWidget>, BlockRecipeSlotWidget> enumerator3 = ValueEnumerableExtensions.GetEnumerator<OfType<FromEnumerable<Widget>, Widget, BlockRecipeSlotWidget>, BlockRecipeSlotWidget>(in ILSpyHelper_AsRefReadOnly(m_resultsItemsWidget.Children.AsValueEnumerable().OfType<BlockRecipeSlotWidget>()));
		try
		{
			while (enumerator3.MoveNext())
			{
				BlockRecipeSlotWidget current3 = enumerator3.Current;
				current3.ClearContents();
				current3.IsCountVisible = BlockRecipeSlotWidget.DefaultIsCountVisible;
				current3.CountTextProcesser = BlockRecipeSlotWidget.DefaultCountTextProcesser;
			}
		}
		finally
		{
			enumerator3.Dispose();
		}
		ValueEnumerator<OfType<FromEnumerable<Widget>, Widget, FluidRecipeSlotWidget>, FluidRecipeSlotWidget> enumerator4 = ValueEnumerableExtensions.GetEnumerator<OfType<FromEnumerable<Widget>, Widget, FluidRecipeSlotWidget>, FluidRecipeSlotWidget>(in ILSpyHelper_AsRefReadOnly(m_resultsFluidsWidget.Children.AsValueEnumerable().OfType<FluidRecipeSlotWidget>()));
		try
		{
			while (enumerator4.MoveNext())
			{
				FluidRecipeSlotWidget current4 = enumerator4.Current;
				current4.ClearContents();
				current4.m_volume = 0f;
			}
		}
		finally
		{
			enumerator4.Dispose();
		}
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
