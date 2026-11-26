using System.Xml.Linq;
using Engine;
using Game;

namespace SCIENEW;

public class CombustionEngineWidget : CanvasWidget
{
	public ComponentCombustionEngine m_componentPump;

	public GridPanelWidget m_inventoryGrid;

	public TankWidget m_tankWidget;

	public FireWidget m_fire;

	public ButtonWidget m_startButton;

	public ButtonWidget m_stopButton;

	public CombustionEngineWidget(IInventory inventory, ComponentCombustionEngine componentPump)
	{
		m_componentPump = componentPump;
		XElement node = ContentManager.Get<XElement>("Widgets/CombustionEngineWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_tankWidget = Children.Find<TankWidget>("Tank");
		m_tankWidget.AssignTank(componentPump, 0);
		m_fire = Children.Find<FireWidget>("Fire");
		m_startButton = Children.Find<ButtonWidget>("StartButton");
		m_stopButton = Children.Find<ButtonWidget>("StopButton");
		int num = 0;
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
