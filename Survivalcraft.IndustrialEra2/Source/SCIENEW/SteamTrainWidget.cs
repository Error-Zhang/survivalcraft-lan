using System.Xml.Linq;
using Engine;
using Game;

namespace SCIENEW;

public class SteamTrainWidget : CanvasWidget
{
	public ComponentSteamEngineV m_componentPump;

	public GridPanelWidget m_inventoryGrid;

	public GridPanelWidget m_engineGrid;

	public TankWidget m_tankWidget;

	public FireWidget m_fire;

	public ButtonWidget m_startButton;

	public ButtonWidget m_stopButton;

	public SteamTrainWidget(IInventory inventory, ComponentSteamEngineV componentPump)
	{
		m_componentPump = componentPump;
		XElement node = ContentManager.Get<XElement>("Widgets/SteamTrainWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_engineGrid = Children.Find<GridPanelWidget>("SteamEngineGrid");
		m_tankWidget = Children.Find<TankWidget>("Tank");
		m_tankWidget.AssignTank(componentPump, 0);
		m_startButton = Children.Find<ButtonWidget>("StartButton");
		m_stopButton = Children.Find<ButtonWidget>("StopButton");
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
		if (m_stopButton.IsClicked)
		{
			IndustrialMachineControl.Change(m_componentPump, IndustrialControl.Running, 0);
		}
		if (m_startButton.IsClicked)
		{
			IndustrialMachineControl.Change(m_componentPump, IndustrialControl.Running, 1);
		}
		m_stopButton.IsChecked = !m_componentPump.start;
		m_startButton.IsChecked = m_componentPump.start;
		m_fire.ParticlesPerSecond = ((m_componentPump.HeatLevel > 0f) ? 24 : 0);
		if (!m_componentPump.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
		}
	}
}
