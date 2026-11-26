using System.Xml.Linq;
using Engine;
using Game;

namespace SCIENEW;

public class SteamBoatWidget : CanvasWidget
{
	public ComponentSteamEngineV m_componentPump;

	public GridPanelWidget m_inventoryGrid;

	public GridPanelWidget m_inventoryGrid2;

	public GridPanelWidget m_engineGrid;

	public TankWidget m_tankWidget;

	public FireWidget m_fire;

	public SteamBoatWidget(IInventory inventory, ComponentSteamEngineV componentPump)
	{
		m_componentPump = componentPump;
		XElement node = ContentManager.Get<XElement>("Widgets/SteamBoatWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_inventoryGrid2 = Children.Find<GridPanelWidget>("FurnaceGrid");
		m_engineGrid = Children.Find<GridPanelWidget>("SteamEngineGrid");
		m_tankWidget = Children.Find<TankWidget>("Tank");
		m_tankWidget.AssignTank(componentPump, 0);
		m_fire = Children.Find<FireWidget>("Fire");
		int num = 0;
		for (int i = 0; i < m_engineGrid.RowsCount; i++)
		{
			for (int j = 0; j < m_engineGrid.ColumnsCount; j++)
			{
				InventorySlotWidget inventorySlotWidget = new InventorySlotWidget();
				inventorySlotWidget.AssignInventorySlot(componentPump, num++);
				m_engineGrid.Children.Add(inventorySlotWidget);
				m_engineGrid.SetWidgetCell(inventorySlotWidget, new Point2(j, i));
			}
		}
		for (int k = 0; k < m_inventoryGrid2.RowsCount; k++)
		{
			for (int l = 0; l < m_inventoryGrid2.ColumnsCount; l++)
			{
				InventorySlotWidget inventorySlotWidget2 = new InventorySlotWidget();
				inventorySlotWidget2.AssignInventorySlot(componentPump, num++);
				m_inventoryGrid2.Children.Add(inventorySlotWidget2);
				m_inventoryGrid2.SetWidgetCell(inventorySlotWidget2, new Point2(l, k));
			}
		}
		num = 10;
		for (int m = 0; m < m_inventoryGrid.RowsCount; m++)
		{
			for (int n = 0; n < m_inventoryGrid.ColumnsCount; n++)
			{
				InventorySlotWidget inventorySlotWidget3 = new InventorySlotWidget();
				inventorySlotWidget3.AssignInventorySlot(inventory, num++);
				m_inventoryGrid.Children.Add(inventorySlotWidget3);
				m_inventoryGrid.SetWidgetCell(inventorySlotWidget3, new Point2(n, m));
			}
		}
	}

	public override void Update()
	{
		m_fire.ParticlesPerSecond = ((m_componentPump.HeatLevel > 0f) ? 24 : 0);
		if (!m_componentPump.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
		}
	}
}
