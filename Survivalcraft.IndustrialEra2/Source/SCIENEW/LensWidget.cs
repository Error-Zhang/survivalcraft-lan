using System.Xml.Linq;
using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class LensWidget : CanvasWidget
{
	public SubsystemTerrain m_subsystemTerrain;

	public ComponentLens m_component;

	public GridPanelWidget m_inventoryGrid;

	public SliderWidget m_yawSlider;

	public SliderWidget m_pitchSlider;

	public ButtonWidget m_plusYawButton;

	public ButtonWidget m_minusYawButton;

	public ButtonWidget m_plusPitchButton;

	public ButtonWidget m_minusPitchButton;

	public LabelWidget m_yawLabel;

	public LabelWidget m_pitchLabel;

	public LensWidget(IInventory inventory, ComponentLens componentDriller)
	{
		m_component = componentDriller;
		m_subsystemTerrain = componentDriller.Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		XElement node = ContentManager.Get<XElement>("Widgets/LensWidget");
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
		m_pitchSlider.MaxValue = 90f;
		m_pitchSlider.MinValue = -90f;
		m_yawSlider.MaxValue = 180f;
		m_yawSlider.MinValue = -180f;
		(int, int) yawPitch = m_component.GetYawPitch();
		m_pitchSlider.Value = yawPitch.Item2;
		m_yawSlider.Value = yawPitch.Item1;
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
		(int, int) yawPitch = m_component.GetYawPitch();
		m_yawLabel.Text = string.Format(LanguageControl.GetContentWidgets("MirrorWidget", 3), yawPitch.Item1.ToString());
		m_pitchLabel.Text = string.Format(LanguageControl.GetContentWidgets("MirrorWidget", 4), yawPitch.Item2.ToString());
		m_plusYawButton.IsEnabled = yawPitch.Item1 < 180;
		m_minusYawButton.IsEnabled = yawPitch.Item1 > -180;
		m_plusPitchButton.IsEnabled = yawPitch.Item2 < 90;
		m_minusPitchButton.IsEnabled = yawPitch.Item2 > -90;
		if (m_plusYawButton.IsClicked)
		{
			IndustrialMachineControl.Change(m_component, IndustrialControl.Yaw, yawPitch.Item1 + 1);
			m_yawSlider.Value = m_component.GetYawPitch().Yaw;
		}
		if (m_minusYawButton.IsClicked)
		{
			IndustrialMachineControl.Change(m_component, IndustrialControl.Yaw, yawPitch.Item1 - 1);
			m_yawSlider.Value = m_component.GetYawPitch().Yaw;
		}
		if (m_plusPitchButton.IsClicked)
		{
			IndustrialMachineControl.Change(m_component, IndustrialControl.Pitch, yawPitch.Item2 + 1);
			m_pitchSlider.Value = m_component.GetYawPitch().Pitch;
		}
		if (m_minusPitchButton.IsClicked)
		{
			IndustrialMachineControl.Change(m_component, IndustrialControl.Pitch, yawPitch.Item2 - 1);
			m_pitchSlider.Value = m_component.GetYawPitch().Pitch;
		}
		IndustrialMachineControl.UpdateSlider(m_component, m_pitchSlider, ref m_pitchWasSliding, yawPitch.Item2, IndustrialControl.Pitch);
		IndustrialMachineControl.UpdateSlider(m_component, m_yawSlider, ref m_yawWasSliding, yawPitch.Item1, IndustrialControl.Yaw);
	}
}
