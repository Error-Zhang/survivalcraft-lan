using System.Xml.Linq;
using Engine;
using Game;

namespace SCIENEW;

public class PetroleumPumpWidget : CanvasWidget
{
	public ComponentPetroleumPump m_componentPump;

	public GridPanelWidget m_inventoryGrid;

	public GridPanelWidget m_pumpGrid;

	public TankWidget m_tank;

	public PetroleumPumpWidget(IInventory inventory, ComponentPetroleumPump componentPump)
	{
		m_componentPump = componentPump;
		XElement node = ContentManager.Get<XElement>("Widgets/PetroleumPumpWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_pumpGrid = Children.Find<GridPanelWidget>("PumpGrid");
		m_tank = Children.Find<TankWidget>("Tank");
		m_tank.AssignTank(componentPump, 0);
		int num = 0;
		for (int i = 0; i < m_pumpGrid.RowsCount; i++)
		{
			for (int j = 0; j < m_pumpGrid.ColumnsCount; j++)
			{
				InventorySlotWidget inventorySlotWidget = new InventorySlotWidget();
				inventorySlotWidget.AssignInventorySlot(componentPump, num++);
				m_pumpGrid.Children.Add(inventorySlotWidget);
				m_pumpGrid.SetWidgetCell(inventorySlotWidget, new Point2(j, i));
			}
		}
		num = 10;
		for (int k = 0; k < m_inventoryGrid.RowsCount; k++)
		{
			for (int l = 0; l < m_inventoryGrid.ColumnsCount; l++)
			{
				InventorySlotWidget inventorySlotWidget2 = new InventorySlotWidget();
				inventorySlotWidget2.AssignInventorySlot(inventory, num++);
				m_inventoryGrid.Children.Add(inventorySlotWidget2);
				m_inventoryGrid.SetWidgetCell(inventorySlotWidget2, new Point2(l, k));
			}
		}
	}

	public override void Update()
	{
		if (!m_componentPump.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
		}
	}
}
