using System;
using System.Xml.Linq;
using Engine;
using Game;
using RecipaediaEX;
using RecipaediaEX.UI;
using SCIENEW.RecipaediaEX.Items;
using SCIENEW.RecipaediaEX.Recipes;
using SCIENEW.Widgets.Recipes.Slots;
using ZLinq;
using ZLinq.Linq;

namespace SCIENEW.Widgets.Recipes.Descriptors;

[RecipeDescriptor(new Type[] { typeof(FractionatingEquationRecipe) }, 0)]
public class FractionatingEquationRecipeDescriptor : RecipeDescriptor
{
	public LabelWidget m_nameWidget;

	public LabelWidget m_descriptionWidget;

	public GridPanelWidget m_materialsFluidsWidget;

	public GridPanelWidget m_resultsFluidsWidget;

	public FractionalWidget m_fractionalWidget;

	public FractionatingEquationRecipeDescriptor(IRecipaediaRecipeNavigator navigator)
		: base(navigator)
	{
		XElement node = ContentManager.Get<XElement>("Widgets/RecipeDescriptors/FractionatingEquationRecipeDescriptor");
		LoadContents(this, node);
		m_nameWidget = Children.Find<LabelWidget>("FractionatingEquationRecipeDescriptor.Name");
		m_descriptionWidget = Children.Find<LabelWidget>("FractionatingEquationRecipeDescriptor.Description");
		m_materialsFluidsWidget = Children.Find<GridPanelWidget>("FractionatingEquationRecipeDescriptor.MaterialsFluids");
		m_resultsFluidsWidget = Children.Find<GridPanelWidget>("FractionatingEquationRecipeDescriptor.ResultsFluids");
		m_fractionalWidget = Children.Find<FractionalWidget>("FractionatingEquationRecipeDescriptor.Fractional");
		for (int i = 0; i < m_materialsFluidsWidget.RowsCount; i++)
		{
			for (int j = 0; j < m_materialsFluidsWidget.ColumnsCount; j++)
			{
				FluidRecipeSlotWidget widget = new FluidRecipeSlotWidget();
				m_materialsFluidsWidget.Children.Add(widget);
				m_materialsFluidsWidget.SetWidgetCell(widget, new Point2(j, i));
			}
		}
		for (int k = 0; k < m_resultsFluidsWidget.RowsCount; k++)
		{
			for (int l = 0; l < m_resultsFluidsWidget.ColumnsCount; l++)
			{
				FluidRecipeSlotWidget widget2 = new FluidRecipeSlotWidget();
				m_resultsFluidsWidget.Children.Add(widget2);
				m_resultsFluidsWidget.SetWidgetCell(widget2, new Point2(l, k));
			}
		}
	}

	public override void Show(IRecipe recipe, string nameSuffix)
	{
		if (!(recipe is FractionatingEquationRecipe fractionatingEquationRecipe))
		{
			return;
		}
		string text = fractionatingEquationRecipe.ResultsFluids.AsValueEnumerable().Aggregate("", (string text2, EquationFluid fluid) => text2 + FluidsManager.GetFluid(fluid.Value).GetDisplayName(fluid.Value) + " ");
		m_nameWidget.Text = text + ((!string.IsNullOrEmpty(nameSuffix)) ? nameSuffix : string.Empty);
		m_descriptionWidget.Text = fractionatingEquationRecipe.Description;
		m_nameWidget.IsVisible = true;
		m_descriptionWidget.IsVisible = true;
		m_fractionalWidget.ParticlesPerSecond = 24f;
		int num = 0;
		ValueEnumerator<OfType<FromEnumerable<Widget>, Widget, FluidRecipeSlotWidget>, FluidRecipeSlotWidget> enumerator = ValueEnumerableExtensions.GetEnumerator<OfType<FromEnumerable<Widget>, Widget, FluidRecipeSlotWidget>, FluidRecipeSlotWidget>(in ILSpyHelper_AsRefReadOnly(m_materialsFluidsWidget.Children.AsValueEnumerable().OfType<FluidRecipeSlotWidget>()));
		try
		{
			while (enumerator.MoveNext())
			{
				FluidRecipeSlotWidget current = enumerator.Current;
				if (num >= fractionatingEquationRecipe.MaterialsFluids.Length)
				{
					current.ClearContents();
					current.m_volume = 0f;
				}
				else
				{
					EquationFluid equationFluid = fractionatingEquationRecipe.MaterialsFluids[num];
					current.SetIngredients(new IRecipaediaItem[1]
					{
						new FluidItem(equationFluid.Value, FluidsManager.GetFluid(equationFluid.Value))
					}, m_navigator);
					current.m_volume = equationFluid.Volume;
				}
				num++;
			}
		}
		finally
		{
			enumerator.Dispose();
		}
		num = 0;
		ValueEnumerator<OfType<FromEnumerable<Widget>, Widget, FluidRecipeSlotWidget>, FluidRecipeSlotWidget> enumerator2 = ValueEnumerableExtensions.GetEnumerator<OfType<FromEnumerable<Widget>, Widget, FluidRecipeSlotWidget>, FluidRecipeSlotWidget>(in ILSpyHelper_AsRefReadOnly(m_resultsFluidsWidget.Children.AsValueEnumerable().OfType<FluidRecipeSlotWidget>()));
		try
		{
			while (enumerator2.MoveNext())
			{
				FluidRecipeSlotWidget current2 = enumerator2.Current;
				if (num >= fractionatingEquationRecipe.ResultsFluids.Length)
				{
					current2.ClearContents();
					current2.m_volume = 0f;
				}
				else
				{
					EquationFluid equationFluid2 = fractionatingEquationRecipe.ResultsFluids[num];
					current2.SetResult(new FluidItem(equationFluid2.Value, FluidsManager.GetFluid(equationFluid2.Value)), m_navigator, equationFluid2.Volume);
				}
				num++;
			}
		}
		finally
		{
			enumerator2.Dispose();
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
		m_fractionalWidget.ParticlesPerSecond = 0f;
		ValueEnumerator<OfType<FromEnumerable<Widget>, Widget, FluidRecipeSlotWidget>, FluidRecipeSlotWidget> enumerator = ValueEnumerableExtensions.GetEnumerator<OfType<FromEnumerable<Widget>, Widget, FluidRecipeSlotWidget>, FluidRecipeSlotWidget>(in ILSpyHelper_AsRefReadOnly(m_materialsFluidsWidget.Children.AsValueEnumerable().OfType<FluidRecipeSlotWidget>()));
		try
		{
			while (enumerator.MoveNext())
			{
				FluidRecipeSlotWidget current = enumerator.Current;
				current.ClearContents();
				current.m_volume = 0f;
			}
		}
		finally
		{
			enumerator.Dispose();
		}
		ValueEnumerator<OfType<FromEnumerable<Widget>, Widget, FluidRecipeSlotWidget>, FluidRecipeSlotWidget> enumerator2 = ValueEnumerableExtensions.GetEnumerator<OfType<FromEnumerable<Widget>, Widget, FluidRecipeSlotWidget>, FluidRecipeSlotWidget>(in ILSpyHelper_AsRefReadOnly(m_resultsFluidsWidget.Children.AsValueEnumerable().OfType<FluidRecipeSlotWidget>()));
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
