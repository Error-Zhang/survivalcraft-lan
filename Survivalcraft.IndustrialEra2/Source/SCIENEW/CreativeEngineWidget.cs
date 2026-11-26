using System.Xml.Linq;
using Engine;
using Game;

namespace SCIENEW;

public class CreativeEngineWidget : CanvasWidget
{
	public SubsystemTerrain m_subsystemTerrain;

	public ComponentCreativeEngine m_component;

	public GridPanelWidget m_inventoryGrid;

	public ButtonWidget m_startButton;

	public ButtonWidget m_stopButton;

	public CreativeEngineWidget(IInventory inventory, ComponentCreativeEngine componentDriller)
	{
		m_component = componentDriller;
		m_subsystemTerrain = componentDriller.Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		XElement node = ContentManager.Get<XElement>("Widgets/CreativeEngineWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_startButton = Children.Find<ButtonWidget>("StartButton");
		m_stopButton = Children.Find<ButtonWidget>("StopButton");
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
		if (m_stopButton.IsClicked)
		{
			IndustrialMachineControl.Change(m_component, IndustrialControl.Running, 0);
		}
		if (m_startButton.IsClicked)
		{
			IndustrialMachineControl.Change(m_component, IndustrialControl.Running, 1);
		}
		m_stopButton.IsChecked = !m_component.m_isPowering;
		m_startButton.IsChecked = m_component.m_isPowering;
		if (!m_component.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
		}
	}
}
