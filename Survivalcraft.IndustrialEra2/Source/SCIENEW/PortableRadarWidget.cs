using System.Xml.Linq;
using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class PortableRadarWidget : CanvasWidget
{
	public GridPanelWidget m_inventoryGrid;

	public SliderWidget m_radiusSlider;

	public ButtonWidget m_plusRadiusButton;

	public ButtonWidget m_minusRadiusButton;

	public GridPanelWidget m_slotsWidget;

	public LabelWidget m_frequencyLabel;

	public ComponentRadar m_radar;

	public PortableRadarWidget(IInventory inventory, ComponentRadar radar, IInventory batteryInventory)
	{
		m_radar = radar;
		XElement node = ContentManager.Get<XElement>("Widgets/PortableRadarWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_radiusSlider = Children.Find<SliderWidget>("RadiusSlider");
		m_plusRadiusButton = Children.Find<ButtonWidget>("PlusRadiusButton");
		m_minusRadiusButton = Children.Find<ButtonWidget>("MinusRadiusButton");
		m_frequencyLabel = Children.Find<LabelWidget>("RadiusLabel");
		m_slotsWidget = Children.Find<GridPanelWidget>("BatteriesGrid");
		int num = 0;
		for (int i = 0; i < m_slotsWidget.RowsCount; i++)
		{
			for (int j = 0; j < m_slotsWidget.ColumnsCount; j++)
			{
				InventorySlotWidget inventorySlotWidget = new InventorySlotWidget();
				inventorySlotWidget.AssignInventorySlot(batteryInventory, num++);
				m_slotsWidget.Children.Add(inventorySlotWidget);
				m_slotsWidget.SetWidgetCell(inventorySlotWidget, new Point2(j, i));
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
		m_radiusSlider.Value = radar.m_radius;
		m_radiusSlider.MinValue = 16f;
		m_radiusSlider.MaxValue = 64f;
		m_frequencyLabel.TextAnchor = TextAnchor.Center;
	}

	private bool m_radiusWasSliding;

	public override void Update()
	{
		m_frequencyLabel.Text = string.Format(LanguageControl.GetContentWidgets("PortableRadarWidget", 3), m_radar.m_radius.ToString());
		m_plusRadiusButton.IsEnabled = m_radar.m_radius < 64;
		m_minusRadiusButton.IsEnabled = m_radar.m_radius > 16;
		if (m_plusRadiusButton.IsClicked)
		{
			IndustrialMachineControl.Change(m_radar, IndustrialControl.RadarRadius, m_radar.m_radius + 1);
			m_radiusSlider.Value = m_radar.m_radius;
		}
		if (m_minusRadiusButton.IsClicked)
		{
			IndustrialMachineControl.Change(m_radar, IndustrialControl.RadarRadius, m_radar.m_radius - 1);
			m_radiusSlider.Value = m_radar.m_radius;
		}
		IndustrialMachineControl.UpdateSlider(m_radar, m_radiusSlider, ref m_radiusWasSliding, m_radar.m_radius, IndustrialControl.RadarRadius);
	}
}
