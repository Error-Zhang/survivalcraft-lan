using System.Xml.Linq;
using Engine;
using Game;

namespace SCIENEW;

public class DrillerWidget : CanvasWidget
{
	public SubsystemTerrain m_subsystemTerrain;

	public ComponentDriller m_componentDriller;

	public ComponentBlockEntity m_componentBlockEntity;

	public GridPanelWidget m_inventoryGrid;

	public GridPanelWidget m_drillerGrid;

	public ButtonWidget m_drillButton;

	public ButtonWidget m_disposeButton;

	public InventorySlotWidget m_drillSlot;

	public InventorySlotWidget m_rodSlot;

	public DrillerWidget(IInventory inventory, ComponentDriller componentDriller)
	{
		m_componentDriller = componentDriller;
		m_componentBlockEntity = componentDriller.Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemTerrain = componentDriller.Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		XElement node = ContentManager.Get<XElement>("Widgets/DrillerWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_drillerGrid = Children.Find<GridPanelWidget>("DrillerGrid");
		m_drillButton = Children.Find<ButtonWidget>("DrillButton");
		m_disposeButton = Children.Find<ButtonWidget>("DisposeButton");
		m_drillSlot = Children.Find<InventorySlotWidget>("DrillSlot");
		m_rodSlot = Children.Find<InventorySlotWidget>("RodSlot");
		m_drillSlot.AssignInventorySlot(componentDriller, componentDriller.DrillerIndex);
		m_rodSlot.AssignInventorySlot(componentDriller, componentDriller.RodSlotIndex);
		int rocksIndex = componentDriller.RocksIndex;
		for (int i = 0; i < m_drillerGrid.RowsCount; i++)
		{
			for (int j = 0; j < m_drillerGrid.ColumnsCount; j++)
			{
				InventorySlotWidget inventorySlotWidget = new InventorySlotWidget();
				inventorySlotWidget.AssignInventorySlot(componentDriller, rocksIndex++);
				m_drillerGrid.Children.Add(inventorySlotWidget);
				m_drillerGrid.SetWidgetCell(inventorySlotWidget, new Point2(j, i));
			}
		}
		rocksIndex = 10;
		for (int k = 0; k < m_inventoryGrid.RowsCount; k++)
		{
			for (int l = 0; l < m_inventoryGrid.ColumnsCount; l++)
			{
				InventorySlotWidget inventorySlotWidget2 = new InventorySlotWidget();
				inventorySlotWidget2.AssignInventorySlot(inventory, rocksIndex++);
				m_inventoryGrid.Children.Add(inventorySlotWidget2);
				m_inventoryGrid.SetWidgetCell(inventorySlotWidget2, new Point2(l, k));
			}
		}
	}

	public override void Update()
	{
		if (m_drillButton.IsClicked)
		{
			IndustrialMachineControl.Change(m_componentDriller, IndustrialControl.OperatingMode, 0);
		}
		if (m_disposeButton.IsClicked)
		{
			IndustrialMachineControl.Change(m_componentDriller, IndustrialControl.OperatingMode, 1);
		}
		DrillerDevice.Mode mode = m_componentDriller.Mode;
		m_drillButton.IsChecked = mode == DrillerDevice.Mode.Drill;
		m_disposeButton.IsChecked = mode == DrillerDevice.Mode.Dispose;
		if (!m_componentDriller.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
		}
	}
}
