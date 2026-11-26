using System.Xml.Linq;
using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class CreativeGeneratorWidget : CanvasWidget
{
	public SubsystemTerrain m_subsystemTerrain;

	public ComponentCreativeGenerator m_component;

	public GridPanelWidget m_inventoryGrid;

	public SliderWidget m_voltageSlider;

	public SliderWidget m_powerSlider;

	public ButtonWidget m_plusVoltageButton;

	public ButtonWidget m_minusVoltageButton;

	public ButtonWidget m_plusPowerButton;

	public ButtonWidget m_minusPowerButton;

	public LabelWidget m_voltageLabel;

	public LabelWidget m_powerLabel;

	private bool m_powerWasSliding;
	private bool m_voltageWasSliding;

	public CreativeGeneratorWidget(IInventory inventory, ComponentCreativeGenerator componentDriller)
	{
		m_component = componentDriller;
		m_subsystemTerrain = componentDriller.Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		XElement node = ContentManager.Get<XElement>("Widgets/CreativeGeneratorWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_voltageSlider = Children.Find<SliderWidget>("VoltageSlider");
		m_powerSlider = Children.Find<SliderWidget>("PowerSlider");
		m_plusVoltageButton = Children.Find<ButtonWidget>("PlusVoltageButton");
		m_minusVoltageButton = Children.Find<ButtonWidget>("MinusVoltageButton");
		m_plusPowerButton = Children.Find<ButtonWidget>("PlusPowerButton");
		m_minusPowerButton = Children.Find<ButtonWidget>("MinusPowerButton");
		m_voltageLabel = Children.Find<LabelWidget>("VoltageLabel");
		m_powerLabel = Children.Find<LabelWidget>("PowerLabel");
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
		m_powerSlider.Value = m_component.m_power;
		m_powerSlider.MaxValue = 64f;
		m_powerSlider.MinValue = 0f;
		m_voltageSlider.Value = m_component.m_voltage;
		m_voltageSlider.MaxValue = 4f;
		m_voltageSlider.MinValue = 0f;
		m_voltageLabel.TextAnchor = TextAnchor.Center;
		m_powerLabel.TextAnchor = TextAnchor.Center;
	}

	public override void Update()
	{
		if (!m_component.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
			return;
		}
		m_voltageLabel.Text = string.Format(LanguageControl.GetContentWidgets("CreativeGeneratorWidget", 3), m_component.m_voltage.ToString("F1"));
		m_powerLabel.Text = string.Format(LanguageControl.GetContentWidgets("CreativeGeneratorWidget", 4), m_component.m_power.ToString("F1"));
		m_plusVoltageButton.IsEnabled = m_component.m_voltage < 4f;
		m_minusVoltageButton.IsEnabled = m_component.m_voltage > 0f;
		m_plusPowerButton.IsEnabled = m_component.m_power < 64f;
		m_minusPowerButton.IsEnabled = m_component.m_power > 0f;
		if (m_plusVoltageButton.IsClicked)
		{
			IndustrialMachineControl.Change(m_component, IndustrialControl.Voltage, m_component.m_voltage + 0.1f);
		}
		if (m_minusVoltageButton.IsClicked)
		{
			IndustrialMachineControl.Change(m_component, IndustrialControl.Voltage, m_component.m_voltage - 0.1f);
		}
		if (m_plusPowerButton.IsClicked)
		{
			IndustrialMachineControl.Change(m_component, IndustrialControl.Power, m_component.m_power + 0.1f);
		}
		if (m_minusPowerButton.IsClicked)
		{
			IndustrialMachineControl.Change(m_component, IndustrialControl.Power, m_component.m_power - 0.1f);
		}
		UpdateSlider(m_powerSlider, ref m_powerWasSliding, m_component.m_power, IndustrialControl.Power);
		UpdateSlider(m_voltageSlider, ref m_voltageWasSliding, m_component.m_voltage, IndustrialControl.Voltage);
	}

	private void UpdateSlider(SliderWidget slider, ref bool wasSliding, float value, IndustrialControl control)
	{
		bool isSliding = slider.IsSliding;
		bool isRemote = IndustrialEraLanAdapter.IsRemote(m_component.Project);
		if ((isRemote && wasSliding && !isSliding) || (!isRemote && isSliding))
		{
			IndustrialMachineControl.Change(m_component, control, slider.Value);
		}
		else if (!isSliding)
		{
			slider.Value = value;
		}
		wasSliding = isSliding;
	}
}
