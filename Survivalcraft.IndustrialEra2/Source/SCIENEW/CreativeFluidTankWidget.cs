using System;
using System.Xml.Linq;
using Engine;
using Game;

namespace SCIENEW;

public class CreativeFluidTankWidget : CanvasWidget
{
	public ComponentCreativeFluidTank m_componentTank;

	public GridPanelWidget m_inventoryGrid;

	public ListPanelWidget m_fluidsList;

	public CreativeFluidTankWidget(IInventory inventory, ComponentCreativeFluidTank componentTank)
	{
		CreativeFluidTankWidget eventsTarget = this;
		m_componentTank = componentTank;
		XElement node = ContentManager.Get<XElement>("Widgets/CreativeFluidTankWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_fluidsList = Children.Find<ListPanelWidget>("FluidsList");
		m_fluidsList.ItemWidgetFactory = (object item) =>
		{
			int num4 = (int)item;
			Fluid fluid = FluidsManager.GetFluid(num4);
			XElement node2 = ContentManager.Get<XElement>("Widgets/RecipaediaFluidItem");
			ContainerWidget containerWidget = (ContainerWidget)Widget.LoadWidget(eventsTarget, node2, null);
			containerWidget.Children.Find<FluidWidget>("RecipaediaItem.Icon").m_fluidValue = num4;
			containerWidget.Children.Find<FluidWidget>("RecipaediaItem.Icon").m_relativeVolume = 1f;
			containerWidget.Children.Find<LabelWidget>("RecipaediaItem.Text").Text = fluid.GetDisplayName(num4);
			string description = fluid.GetDescription(num4);
			description = description.Replace("\n", "  ");
			containerWidget.Children.Find<LabelWidget>("RecipaediaItem.Details").Text = description;
			return containerWidget;
		};
		ListPanelWidget fluidsList = m_fluidsList;
		fluidsList.ItemClicked = (Action<object>)Delegate.Combine(fluidsList.ItemClicked, (Action<object>)((object item) =>
		{
			if (item is int fluidValue)
			{
				componentTank.FluidValue = fluidValue;
			}
		}));
		int num = 10;
		for (int num2 = 0; num2 < m_inventoryGrid.RowsCount; num2++)
		{
			for (int num3 = 0; num3 < m_inventoryGrid.ColumnsCount; num3++)
			{
				InventorySlotWidget inventorySlotWidget = new InventorySlotWidget();
				inventorySlotWidget.AssignInventorySlot(inventory, num++);
				m_inventoryGrid.Children.Add(inventorySlotWidget);
				m_inventoryGrid.SetWidgetCell(inventorySlotWidget, new Point2(num3, num2));
			}
		}
		PopulateFluidsList();
	}

	public void PopulateFluidsList()
	{
		m_fluidsList.ScrollPosition = 0f;
		m_fluidsList.ClearItems();
		Fluid[] fluids = FluidsManager.Fluids;
		foreach (Fluid fluid in fluids)
		{
			m_fluidsList.AddItem(FluidsManager.GetFluidID(fluid));
		}
		m_fluidsList.SelectedItem = m_componentTank.FluidValue;
	}

	public override void Update()
	{
		if (!m_componentTank.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
		}
	}
}
