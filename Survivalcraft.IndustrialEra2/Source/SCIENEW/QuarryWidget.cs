using System.Xml.Linq;
using Engine;
using Game;

namespace SCIENEW;

public class QuarryWidget : CanvasWidget
{
	public SubsystemTerrain m_subsystemTerrain;

	public ComponentQuarry m_componentDriller;

	public ComponentBlockEntity m_componentBlockEntity;

	public GridPanelWidget m_inventoryGrid;

	public CanvasWidget m_preparingPanel;

	public ButtonWidget m_selectAreaButton;

	public ButtonWidget m_confirmDrillButton;

	public LabelWidget m_areaLabel;

	public CanvasWidget m_drillingPanel;

	public GridPanelWidget m_drillerGrid;

	public ButtonWidget m_drillButton;

	public ButtonWidget m_disposeButton;

	public InventorySlotWidget m_drillSlot;

	public CheckboxWidget m_drawAreaBox;

	public QuarryWidget(IInventory inventory, ComponentQuarry componentDriller)
	{
		m_componentDriller = componentDriller;
		m_componentBlockEntity = componentDriller.Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemTerrain = componentDriller.Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		XElement node = ContentManager.Get<XElement>("Widgets/QuarryWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_preparingPanel = Children.Find<CanvasWidget>("PreparingPanel");
		m_areaLabel = Children.Find<LabelWidget>("AreaLabel");
		m_confirmDrillButton = Children.Find<ButtonWidget>("ConfirmDrillButton");
		m_selectAreaButton = Children.Find<ButtonWidget>("SelectAreaButton");
		m_drillingPanel = Children.Find<CanvasWidget>("DrillingPanel");
		m_drillerGrid = Children.Find<GridPanelWidget>("DrillerGrid");
		m_drillButton = Children.Find<ButtonWidget>("DrillButton");
		m_disposeButton = Children.Find<ButtonWidget>("DisposeButton");
		m_drillSlot = Children.Find<InventorySlotWidget>("DrillSlot");
		m_drawAreaBox = Children.Find<CheckboxWidget>("DrawAreaBox");
		m_drillingPanel.IsVisible = false;
		m_preparingPanel.IsVisible = true;
		m_drillSlot.AssignInventorySlot(componentDriller, 0);
		int num = 1;
		for (int i = 0; i < m_drillerGrid.RowsCount; i++)
		{
			for (int j = 0; j < m_drillerGrid.ColumnsCount; j++)
			{
				InventorySlotWidget inventorySlotWidget = new InventorySlotWidget();
				inventorySlotWidget.AssignInventorySlot(componentDriller, num++);
				m_drillerGrid.Children.Add(inventorySlotWidget);
				m_drillerGrid.SetWidgetCell(inventorySlotWidget, new Point2(j, i));
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
		if (m_selectAreaButton.IsClicked)
		{
			IndustrialMachineControl.Change(m_componentDriller, IndustrialControl.SelectArea, 1);
		}
		if (m_confirmDrillButton.IsClicked)
		{
			IndustrialMachineControl.Change(m_componentDriller, IndustrialControl.ConfirmArea, 1);
		}
		m_preparingPanel.IsVisible = m_componentDriller.IsPreparing;
		m_drillingPanel.IsVisible = !m_componentDriller.IsPreparing;
		m_areaLabel.Text = (m_componentDriller.m_area.HasValue ? m_componentDriller.m_area.Value.ToString() : string.Empty);
		m_confirmDrillButton.IsEnabled = m_componentDriller.m_area.HasValue;
		if (m_drillButton.IsClicked)
		{
			IndustrialMachineControl.Change(m_componentDriller, IndustrialControl.OperatingMode, 0);
		}
		if (m_disposeButton.IsClicked)
		{
			IndustrialMachineControl.Change(m_componentDriller, IndustrialControl.OperatingMode, 1);
		}
		if (m_drawAreaBox.IsClicked)
		{
			m_componentDriller.m_displayArea = !m_componentDriller.m_displayArea;
		}
		QuarryDevice.Mode mode = m_componentDriller.Mode;
		m_drillButton.IsChecked = mode == QuarryDevice.Mode.Drill;
		m_disposeButton.IsChecked = mode == QuarryDevice.Mode.Dispose;
		m_drawAreaBox.IsChecked = m_componentDriller.m_displayArea;
		if (!m_componentDriller.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
		}
	}
}
