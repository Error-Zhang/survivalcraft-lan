using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Game;
using RecipaediaEX.UI;
using SCIENEW.RecipaediaEX.Items;
using SCIENEW.Utils;

namespace SCIENEW.RecipaediaEX.Categories;

public class FluidCategory : IRecipaediaCategory
{
	private const string m_fluidCategory = "Fluid";

	public List<FluidItem> m_fluidItems;

	public string Id => "Fluid";

	public string DisplayName => LanguageUtils.GetText(this, "Fluid");

	public Func<IRecipaediaItem, Widget> ItemWidgetFactory => CreateItemWidget;

	public IEnumerable<IRecipaediaItem> GetItems()
	{
		return m_fluidItems;
	}

	public FluidCategory()
	{
		m_fluidItems = new List<FluidItem>();
		Fluid[] fluids = FluidsManager.Fluids;
		foreach (Fluid fluid in fluids)
		{
			int fluidID = FluidsManager.GetFluidID(fluid);
			m_fluidItems.Add(new FluidItem(fluidID, fluid));
		}
	}

	private Widget CreateItemWidget(IRecipaediaItem item)
	{
		if (!(item is FluidItem fluidItem) || !(fluidItem.Value is int num))
		{
			return null;
		}
		Fluid fluid = fluidItem.m_fluid;
		XElement node = ContentManager.Get<XElement>("Widgets/RecipaediaFluidItem");
		ContainerWidget containerWidget = (ContainerWidget)Widget.LoadWidget(this, node, null);
		containerWidget.Children.Find<FluidWidget>("RecipaediaItem.Icon").m_fluidValue = num;
		containerWidget.Children.Find<FluidWidget>("RecipaediaItem.Icon").m_relativeVolume = 1f;
		containerWidget.Children.Find<LabelWidget>("RecipaediaItem.Text").Text = fluid.GetDisplayName(num);
		string description = fluid.GetDescription(num);
		description = description.Replace("\n", "  ");
		containerWidget.Children.Find<LabelWidget>("RecipaediaItem.Details").Text = description;
		return containerWidget;
	}
}
