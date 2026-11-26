using System.Xml.Linq;
using Engine;
using Game;

namespace SCIENEW;

public class LiquidPumpWidget : CanvasWidget
{
	public ComponentLiquidPump m_componentPump;

	public GridPanelWidget m_inventoryGrid;

	public GridPanelWidget m_pumpGrid;

	public GridPanelWidget m_tanksGird;

	public LiquidPumpWidget(IInventory inventory, ComponentLiquidPump componentPump)
	{
		m_componentPump = componentPump;
		XElement node = ContentManager.Get<XElement>("Widgets/LiquidPumpWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_pumpGrid = Children.Find<GridPanelWidget>("PumpGrid");
		m_tanksGird = Children.Find<GridPanelWidget>("TanksGrid");
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
		num = 0;
		for (int k = 0; k < m_tanksGird.RowsCount; k++)
		{
			for (int l = 0; l < m_tanksGird.ColumnsCount; l++)
			{
				TankWidget tankWidget = new TankWidget
				{
					Size = new Vector2(72f, 216f)
				};
				tankWidget.AssignTank(componentPump, num++);
				m_tanksGird.Children.Add(tankWidget);
				m_tanksGird.SetWidgetCell(tankWidget, new Point2(l, k));
			}
		}
		num = 10;
		for (int m = 0; m < m_inventoryGrid.RowsCount; m++)
		{
			for (int n = 0; n < m_inventoryGrid.ColumnsCount; n++)
			{
				InventorySlotWidget inventorySlotWidget2 = new InventorySlotWidget();
				inventorySlotWidget2.AssignInventorySlot(inventory, num++);
				m_inventoryGrid.Children.Add(inventorySlotWidget2);
				m_inventoryGrid.SetWidgetCell(inventorySlotWidget2, new Point2(n, m));
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
