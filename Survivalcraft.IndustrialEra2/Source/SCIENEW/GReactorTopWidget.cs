using System.Xml.Linq;
using Engine;
using Game;

namespace SCIENEW;

public class GReactorTopWidget : CanvasWidget
{
	public ComponentGReactorTop m_componentChest;

	public GridPanelWidget m_inventoryGrid;

	public GridPanelWidget m_chestGrid;

	public LabelWidget m_label;

	public GReactorTopWidget(IInventory inventory, ComponentGReactorTop componentChest)
	{
		m_componentChest = componentChest;
		XElement node = ContentManager.Get<XElement>("Widgets/GReactorTopWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_chestGrid = Children.Find<GridPanelWidget>("ChestGrid");
		m_label = Children.Find<LabelWidget>("TopLabel");
		int num = 0;
		for (int i = 0; i < m_chestGrid.RowsCount; i++)
		{
			for (int j = 0; j < m_chestGrid.ColumnsCount; j++)
			{
				InventorySlotWidget inventorySlotWidget = new InventorySlotWidget();
				inventorySlotWidget.AssignInventorySlot(componentChest, num++);
				m_chestGrid.Children.Add(inventorySlotWidget);
				m_chestGrid.SetWidgetCell(inventorySlotWidget, new Point2(j, i));
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
		if (!m_componentChest.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
		}
	}
}
