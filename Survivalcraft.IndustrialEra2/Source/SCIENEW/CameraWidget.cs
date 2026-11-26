using System.Xml.Linq;
using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class CameraWidget : CanvasWidget
{
	public SubsystemTerrain m_subsystemTerrain;

	public ComponentCamera m_component;

	public GridPanelWidget m_inventoryGrid;

	public SliderWidget m_yawSlider;

	public SliderWidget m_pitchSlider;

	public ButtonWidget m_plusYawButton;

	public ButtonWidget m_minusYawButton;

	public ButtonWidget m_plusPitchButton;

	public ButtonWidget m_minusPitchButton;

	public LabelWidget m_yawLabel;

	public LabelWidget m_pitchLabel;

	public CameraWidget(IInventory inventory, ComponentCamera componentDriller)
	{
		m_component = componentDriller;
		m_subsystemTerrain = componentDriller.Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		XElement node = ContentManager.Get<XElement>("Widgets/CameraWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_yawSlider = Children.Find<SliderWidget>("YawSlider");
		m_pitchSlider = Children.Find<SliderWidget>("PitchSlider");
		m_plusYawButton = Children.Find<ButtonWidget>("PlusYawButton");
		m_minusYawButton = Children.Find<ButtonWidget>("MinusYawButton");
		m_plusPitchButton = Children.Find<ButtonWidget>("PlusPitchButton");
		m_minusPitchButton = Children.Find<ButtonWidget>("MinusPitchButton");
		m_yawLabel = Children.Find<LabelWidget>("YawLabel");
		m_pitchLabel = Children.Find<LabelWidget>("PitchLabel");
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
		m_pitchSlider.MaxValue = 60f;
		m_pitchSlider.MinValue = -60f;
		m_yawSlider.MaxValue = 180f;
		m_yawSlider.MinValue = -180f;
		m_pitchSlider.Value = m_component.Pitch;
		m_yawSlider.Value = m_component.Yaw;
		m_yawLabel.TextAnchor = TextAnchor.Center;
		m_pitchLabel.TextAnchor = TextAnchor.Center;
	}

	private bool m_yawWasSliding;
	private bool m_pitchWasSliding;

	public override void Update()
	{
		if (!m_component.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
			return;
		}
		m_yawLabel.Text = string.Format(LanguageControl.GetContentWidgets("CameraWidget", 3), m_component.Yaw.ToString());
		m_pitchLabel.Text = string.Format(LanguageControl.GetContentWidgets("CameraWidget", 4), m_component.Pitch.ToString());
		m_plusYawButton.IsEnabled = m_component.Yaw < 180;
		m_minusYawButton.IsEnabled = m_component.Yaw > -180;
		m_plusPitchButton.IsEnabled = m_component.Pitch < 60;
		m_minusPitchButton.IsEnabled = m_component.Pitch > -60;
		if (m_plusYawButton.IsClicked)
		{
			IndustrialMachineControl.Change(m_component, IndustrialControl.Yaw, m_component.Yaw + 1);
			m_yawSlider.Value = m_component.Yaw;
		}
		if (m_minusYawButton.IsClicked)
		{
			IndustrialMachineControl.Change(m_component, IndustrialControl.Yaw, m_component.Yaw - 1);
			m_yawSlider.Value = m_component.Yaw;
		}
		if (m_plusPitchButton.IsClicked)
		{
			IndustrialMachineControl.Change(m_component, IndustrialControl.Pitch, m_component.Pitch + 1);
			m_pitchSlider.Value = m_component.Pitch;
		}
		if (m_minusPitchButton.IsClicked)
		{
			IndustrialMachineControl.Change(m_component, IndustrialControl.Pitch, m_component.Pitch - 1);
			m_pitchSlider.Value = m_component.Pitch;
		}
		IndustrialMachineControl.UpdateSlider(m_component, m_pitchSlider, ref m_pitchWasSliding, m_component.Pitch, IndustrialControl.Pitch);
		IndustrialMachineControl.UpdateSlider(m_component, m_yawSlider, ref m_yawWasSliding, m_component.Yaw, IndustrialControl.Yaw);
	}
}
