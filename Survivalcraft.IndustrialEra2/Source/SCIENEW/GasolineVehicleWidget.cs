using System.Xml.Linq;
using Engine;
using Game;

namespace SCIENEW;

public class GasolineVehicleWidget : CanvasWidget
{
	public ComponentCombustionEngine m_componentPump;

	public GridPanelWidget m_inventoryGrid;

	public GridPanelWidget m_inventoryGrid2;

	public TankWidget m_tankWidget;

	public FireWidget m_fire;

	public LabelWidget m_title;

	public ButtonWidget m_startButton;

	public ButtonWidget m_stopButton;

	public GasolineVehicleWidget(IInventory inventory, ComponentCombustionEngine componentPump, string title)
	{
		m_componentPump = componentPump;
		XElement node = ContentManager.Get<XElement>("Widgets/GasolineVehicleWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_inventoryGrid2 = Children.Find<GridPanelWidget>("InnerChestGrid");
		m_tankWidget = Children.Find<TankWidget>("Tank");
		m_tankWidget.AssignTank(componentPump, 0);
		m_fire = Children.Find<FireWidget>("Fire");
		m_startButton = Children.Find<ButtonWidget>("StartButton");
		m_stopButton = Children.Find<ButtonWidget>("StopButton");
		m_title = Children.Find<LabelWidget>("Title");
		m_title.Text = title;
		int num = 0;
		for (int i = 0; i < m_inventoryGrid2.RowsCount; i++)
		{
			for (int j = 0; j < m_inventoryGrid2.ColumnsCount; j++)
			{
				InventorySlotWidget inventorySlotWidget = new InventorySlotWidget();
				inventorySlotWidget.AssignInventorySlot(componentPump, num++);
				m_inventoryGrid2.Children.Add(inventorySlotWidget);
				m_inventoryGrid2.SetWidgetCell(inventorySlotWidget, new Point2(j, i));
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
		m_fire.ParticlesPerSecond = ((m_componentPump.HeatLevel > 0f) ? 24 : 0);
		if (m_startButton.IsClicked)
		{
			IndustrialMachineControl.Change(m_componentPump, IndustrialControl.Running, 1);
		}
		if (m_stopButton.IsClicked)
		{
			IndustrialMachineControl.Change(m_componentPump, IndustrialControl.Running, 0);
		}
		m_startButton.IsChecked = m_componentPump.start;
		m_stopButton.IsChecked = !m_componentPump.start;
		if (!m_componentPump.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
		}
	}
}
