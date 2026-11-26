using System.Xml.Linq;
using Engine;
using Game;

namespace SCIENEW;

public class CreativeChestWidget : CanvasWidget
{
	public SubsystemTerrain m_subsystemTerrain;

	public ComponentCreativeChest m_component;

	public ComponentBlockEntity m_componentBlockEntity;

	public GridPanelWidget m_inventoryGrid;

	public GridPanelWidget m_itemGrid;

	public ButtonWidget m_shootButton;

	public ButtonWidget m_dispenseButton;

	public CreativeChestWidget(IInventory inventory, ComponentCreativeChest component)
	{
		m_component = component;
		m_componentBlockEntity = component.Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemTerrain = component.Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		XElement node = ContentManager.Get<XElement>("Widgets/CreativeChestWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_itemGrid = Children.Find<GridPanelWidget>("ItemGrid");
		m_shootButton = Children.Find<ButtonWidget>("ShootButton");
		m_dispenseButton = Children.Find<ButtonWidget>("DispenseButton");
		int refSlotIndex = m_component.RefSlotIndex;
		for (int i = 0; i < m_itemGrid.RowsCount; i++)
		{
			for (int j = 0; j < m_itemGrid.ColumnsCount; j++)
			{
				InventorySlotWidget inventorySlotWidget = new InventorySlotWidget();
				inventorySlotWidget.AssignInventorySlot(component, refSlotIndex++);
				m_itemGrid.Children.Add(inventorySlotWidget);
				m_itemGrid.SetWidgetCell(inventorySlotWidget, new Point2(j, i));
			}
		}
		refSlotIndex = 10;
		for (int k = 0; k < m_inventoryGrid.RowsCount; k++)
		{
			for (int l = 0; l < m_inventoryGrid.ColumnsCount; l++)
			{
				InventorySlotWidget inventorySlotWidget2 = new InventorySlotWidget();
				inventorySlotWidget2.AssignInventorySlot(inventory, refSlotIndex++);
				m_inventoryGrid.Children.Add(inventorySlotWidget2);
				m_inventoryGrid.SetWidgetCell(inventorySlotWidget2, new Point2(l, k));
			}
		}
	}

	public override void Update()
	{
		if (m_shootButton.IsClicked)
		{
			IndustrialMachineControl.Change(m_component, IndustrialControl.OperatingMode, 0);
		}
		if (m_dispenseButton.IsClicked)
		{
			IndustrialMachineControl.Change(m_component, IndustrialControl.OperatingMode, 1);
		}
		CreativeChestDevice.Mode mode = m_component.m_mode;
		m_shootButton.IsChecked = mode == CreativeChestDevice.Mode.Shoot;
		m_dispenseButton.IsChecked = mode == CreativeChestDevice.Mode.Dispense;
		if (!m_component.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
		}
	}
}
