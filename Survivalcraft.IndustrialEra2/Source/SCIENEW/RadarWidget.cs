using System.Xml.Linq;
using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class RadarWidget : CanvasWidget
{
	public GridPanelWidget m_inventoryGrid;

	public SliderWidget m_radiusSlider;

	public ButtonWidget m_plusRadiusButton;

	public ButtonWidget m_minusRadiusButton;

	public LabelWidget m_frequencyLabel;

	public ComponentRadar m_radar;

	public RadarWidget(IInventory inventory, ComponentRadar radar)
	{
		m_radar = radar;
		XElement node = ContentManager.Get<XElement>("Widgets/RadarWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_radiusSlider = Children.Find<SliderWidget>("RadiusSlider");
		m_plusRadiusButton = Children.Find<ButtonWidget>("PlusRadiusButton");
		m_minusRadiusButton = Children.Find<ButtonWidget>("MinusRadiusButton");
		m_frequencyLabel = Children.Find<LabelWidget>("RadiusLabel");
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
		m_radiusSlider.Value = radar.m_radius;
		m_radiusSlider.MinValue = 16f;
		m_radiusSlider.MaxValue = 64f;
		m_frequencyLabel.TextAnchor = TextAnchor.Center;
	}

	private bool m_radiusWasSliding;

	public override void Update()
	{
		m_frequencyLabel.Text = string.Format(LanguageControl.GetContentWidgets("RadarWidget", 3), m_radar.m_radius.ToString());
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
