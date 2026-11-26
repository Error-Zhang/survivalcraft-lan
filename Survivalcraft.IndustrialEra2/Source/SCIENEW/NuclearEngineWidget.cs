using System.Xml.Linq;
using Engine;
using Game;

namespace SCIENEW;

public class NuclearEngineWidget : CanvasWidget
{
	public GridPanelWidget m_inventoryGrid;

	public InventorySlotWidget m_rodSlot;

	public ComponentNuclearEngine m_componentNuclearEngine;

	public BevelledButtonWidget m_startButton;

	public BevelledButtonWidget m_stopButton;

	public NuclearFireWidget m_fireWidget;

	public NuclearEngineWidget(IInventory inventory, ComponentNuclearEngine component)
	{
		m_componentNuclearEngine = component;
		XElement node = ContentManager.Get<XElement>("Widgets/NuclearEngineWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_startButton = Children.Find<BevelledButtonWidget>("StartButton");
		m_stopButton = Children.Find<BevelledButtonWidget>("StopButton");
		m_rodSlot = Children.Find<InventorySlotWidget>("RodSlot");
		m_fireWidget = Children.Find<NuclearFireWidget>("Fire");
		m_startButton.IsAutoCheckingEnabled = true;
		m_stopButton.IsAutoCheckingEnabled = true;
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
		m_rodSlot.AssignInventorySlot(component, 0);
	}

	public override void Update()
	{
		if (m_startButton.IsClicked)
		{
			IndustrialMachineControl.Change(m_componentNuclearEngine, IndustrialControl.Running, 1);
		}
		if (m_stopButton.IsClicked)
		{
			IndustrialMachineControl.Change(m_componentNuclearEngine, IndustrialControl.Running, 0);
		}
		m_stopButton.IsChecked = !m_componentNuclearEngine.Ignited;
		m_startButton.IsChecked = m_componentNuclearEngine.Ignited;
		m_fireWidget.ParticlesPerSecond = (m_componentNuclearEngine.IsPowering() ? 24 : 0);
		if (!m_componentNuclearEngine.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
		}
	}
}
