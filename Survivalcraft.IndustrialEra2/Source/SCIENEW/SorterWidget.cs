using System.Xml.Linq;
using Engine;
using Game;

namespace SCIENEW;

public class SorterWidget : CanvasWidget
{
	public GridPanelWidget m_inventoryGrid;

	public InventorySlotWidget m_resultSlot;

	public InventorySlotWidget rawSlot;

	public ValueBarWidget m_progress;

	public ComponentInventoryBase m_presser;

	protected readonly InventorySlotWidget m_slot1;

	protected readonly InventorySlotWidget m_slot2;

	protected readonly InventorySlotWidget m_slot3;

	protected readonly InventorySlotWidget m_slot4;

	public SorterWidget(IInventory inventory, ComponentInventoryBase component)
	{
		m_presser = component;
		XElement node = ContentManager.Get<XElement>("Widgets/SorterWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_slot1 = Children.Find<InventorySlotWidget>("Slot1");
		m_slot2 = Children.Find<InventorySlotWidget>("Slot2");
		m_slot3 = Children.Find<InventorySlotWidget>("Slot3");
		m_slot4 = Children.Find<InventorySlotWidget>("Slot4");
		int num = 0;
		m_slot1.AssignInventorySlot(component, num++);
		m_slot2.AssignInventorySlot(component, num++);
		m_slot3.AssignInventorySlot(component, num++);
		m_slot4.AssignInventorySlot(component, num++);
		num = 10;
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
}
