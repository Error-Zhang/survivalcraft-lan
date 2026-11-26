using System.Xml.Linq;
using Engine;
using Game;

namespace SCIENEW;

public class CastingCheckValveWidget : CanvasWidget
{
	public ComponentCastingCheckValve m_componentPump;

	public GridPanelWidget m_inventoryGrid;

	public TankWidget m_tankWidget;

	public CastingCheckValveWidget(IInventory inventory, ComponentCastingCheckValve componentPump)
	{
		m_componentPump = componentPump;
		XElement node = ContentManager.Get<XElement>("Widgets/CastingCheckValveWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_tankWidget = Children.Find<TankWidget>("Tank");
		m_tankWidget.AssignTank(componentPump, 0);
		int num = 10;
		for (int i = 0; i < m_inventoryGrid.RowsCount; i++)
		{
			for (int j = 0; j < m_inventoryGrid.ColumnsCount; j++)
			{
				InventorySlotWidget inventorySlotWidget = new InventorySlotWidget();
				inventorySlotWidget.AssignInventorySlot(inventory, num++);
				m_inventoryGrid.Children.Add(inventorySlotWidget);
				m_inventoryGrid.SetWidgetCell(inventorySlotWidget, new Point2(j, i));
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
