using System.Xml.Linq;
using Engine;
using Game;

namespace SCIENEW;

public class SolarPanelBaseWidget : CanvasWidget
{
	public SubsystemTerrain m_subsystemTerrain;

	public ComponentSolarPanelBase m_component;

	public GridPanelWidget m_inventoryGrid;

	public SolarPanelBaseWidget(IInventory inventory, ComponentSolarPanelBase componentDriller)
	{
		m_component = componentDriller;
		m_subsystemTerrain = componentDriller.Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		XElement node = ContentManager.Get<XElement>("Widgets/SolarPanelBaseWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
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
		if (!m_component.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
		}
	}
}
