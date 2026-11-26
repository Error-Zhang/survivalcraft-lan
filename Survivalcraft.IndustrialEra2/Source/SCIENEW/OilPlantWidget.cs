using System.Xml.Linq;
using Engine;
using Game;

namespace SCIENEW;

public class OilPlantWidget : CanvasWidget
{
	public ComponentOilPlant m_componentPump;

	public GridPanelWidget m_inventoryGrid;

	public GridPanelWidget m_pumpGrid;

	public GridPanelWidget m_tanksGird;

	public GridPanelWidget m_tanksGird2;

	public ValueBarWidget m_progress;

	public InventorySlotWidget m_resultSlot1;

	public InventorySlotWidget m_resultSlot2;

	public InventorySlotWidget m_resultSlot3;

	public OilPlantWidget(IInventory inventory, ComponentOilPlant componentPump)
	{
		m_componentPump = componentPump;
		XElement node = ContentManager.Get<XElement>("Widgets/OilPlantWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_pumpGrid = Children.Find<GridPanelWidget>("FurnaceGrid");
		m_progress = Children.Find<ValueBarWidget>("Progress");
		m_tanksGird = Children.Find<GridPanelWidget>("TanksGrid");
		m_tanksGird2 = Children.Find<GridPanelWidget>("TanksGrid2");
		m_resultSlot1 = Children.Find<InventorySlotWidget>("ResultSlot1");
		m_resultSlot2 = Children.Find<InventorySlotWidget>("ResultSlot2");
		m_resultSlot3 = Children.Find<InventorySlotWidget>("ResultSlot3");
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
		m_resultSlot1.AssignInventorySlot(componentPump, num++);
		m_resultSlot2.AssignInventorySlot(componentPump, num++);
		m_resultSlot3.AssignInventorySlot(componentPump, num++);
		num = 0;
		for (int k = 0; k < m_tanksGird.RowsCount; k++)
		{
			for (int l = 0; l < m_tanksGird.ColumnsCount; l++)
			{
				TankWidget tankWidget = new TankWidget
				{
					Size = new Vector2(72f, 72f)
				};
				tankWidget.AssignTank(componentPump, num++);
				m_tanksGird.Children.Add(tankWidget);
				m_tanksGird.SetWidgetCell(tankWidget, new Point2(l, k));
			}
		}
		for (int m = 0; m < m_tanksGird2.RowsCount; m++)
		{
			for (int n = 0; n < m_tanksGird2.ColumnsCount; n++)
			{
				TankWidget tankWidget2 = new TankWidget
				{
					Size = new Vector2(72f, 72f)
				};
				tankWidget2.AssignTank(componentPump, num++);
				m_tanksGird2.Children.Add(tankWidget2);
				m_tanksGird2.SetWidgetCell(tankWidget2, new Point2(n, m));
			}
		}
		num = 10;
		for (int num2 = 0; num2 < m_inventoryGrid.RowsCount; num2++)
		{
			for (int num3 = 0; num3 < m_inventoryGrid.ColumnsCount; num3++)
			{
				InventorySlotWidget inventorySlotWidget2 = new InventorySlotWidget();
				inventorySlotWidget2.AssignInventorySlot(inventory, num++);
				m_inventoryGrid.Children.Add(inventorySlotWidget2);
				m_inventoryGrid.SetWidgetCell(inventorySlotWidget2, new Point2(num3, num2));
			}
		}
	}

	public override void Update()
	{
		m_progress.Value = m_componentPump.m_craftingProgress;
		if (!m_componentPump.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
		}
	}
}
