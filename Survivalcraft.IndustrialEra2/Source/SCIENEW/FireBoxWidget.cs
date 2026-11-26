using System.Xml.Linq;
using Engine;
using Game;

namespace SCIENEW;

public class FireBoxWidget : CanvasWidget
{
	public ComponentFireBox m_componentStirlingEngine;

	public GridPanelWidget m_inventoryGrid;

	public GridPanelWidget m_engineGrid;

	public InventorySlotWidget m_ashesSlot;

	public FireWidget m_fire;

	public ValueBarWidget m_bar;

	public FireBoxWidget(IInventory inventory, ComponentFireBox componentStirlingEngine)
	{
		m_componentStirlingEngine = componentStirlingEngine;
		XElement node = ContentManager.Get<XElement>("Widgets/FireBoxWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_engineGrid = Children.Find<GridPanelWidget>("SteamEngineGrid");
		m_fire = Children.Find<FireWidget>("Fire");
		m_bar = Children.Find<ValueBarWidget>("Progress");
		m_ashesSlot = Children.Find<InventorySlotWidget>("AshesSlot");
		int num = 0;
		for (int i = 0; i < m_engineGrid.RowsCount; i++)
		{
			for (int j = 0; j < m_engineGrid.ColumnsCount; j++)
			{
				InventorySlotWidget inventorySlotWidget = new InventorySlotWidget();
				inventorySlotWidget.AssignInventorySlot(componentStirlingEngine, num++);
				m_engineGrid.Children.Add(inventorySlotWidget);
				m_engineGrid.SetWidgetCell(inventorySlotWidget, new Point2(j, i));
			}
		}
		m_ashesSlot.AssignInventorySlot(componentStirlingEngine, componentStirlingEngine.AshesSlotIndex);
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
		m_bar.Value = ((m_componentStirlingEngine.FireTime == 0f) ? 0f : (m_componentStirlingEngine.FireTimeRemaining / m_componentStirlingEngine.FireTime));
		m_fire.ParticlesPerSecond = ((m_componentStirlingEngine.HeatLevel > 0f) ? 24 : 0);
		if (!m_componentStirlingEngine.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
		}
	}
}
