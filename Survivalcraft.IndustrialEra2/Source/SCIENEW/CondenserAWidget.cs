using System.Xml.Linq;
using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class CondenserAWidget : CanvasWidget
{
	public GridPanelWidget m_inventoryGrid;

	public ComponentCondenserA m_condenser;

	public LabelWidget m_powerLabel;

	public BevelledButtonWidget m_chargeButton;

	public BevelledButtonWidget m_dischargeButton;

	public ChargeWidget m_chargeWidget;

	public CheckboxWidget m_electricalCheckbox;

	public SliderWidget m_powerSlider;

	public ButtonWidget m_plusPowerButton;

	public ButtonWidget m_minusPowerButton;

	public CondenserAWidget(IInventory inventory, ComponentCondenserA condenser)
	{
		m_condenser = condenser;
		XElement node = ContentManager.Get<XElement>("Widgets/CondenserAWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_powerLabel = Children.Find<LabelWidget>("PowerLabel");
		m_chargeButton = Children.Find<BevelledButtonWidget>("ChargeButton");
		m_dischargeButton = Children.Find<BevelledButtonWidget>("DischargeButton");
		m_chargeWidget = Children.Find<ChargeWidget>("Charge");
		m_electricalCheckbox = Children.Find<CheckboxWidget>("ElectricalControl");
		m_powerSlider = Children.Find<SliderWidget>("PowerSlider");
		m_plusPowerButton = Children.Find<ButtonWidget>("PlusPowerButton");
		m_minusPowerButton = Children.Find<ButtonWidget>("MinusPowerButton");
		m_chargeButton.IsAutoCheckingEnabled = true;
		m_dischargeButton.IsAutoCheckingEnabled = true;
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
		m_powerSlider.MinValue = ComponentCondenserA.MinOutputPower;
		m_powerSlider.MaxValue = ComponentCondenserA.MaxOutputPower;
		m_powerSlider.Value = m_condenser.OutputPower;
		m_powerSlider.Granularity = 0.1f;
		m_powerLabel.TextAnchor = TextAnchor.Center;
		m_powerSlider.Children.Find<RectangleWidget>("Slider.Rectangle").Size = new Vector2(6f, float.PositiveInfinity);
	}

	public override void Update()
	{
		if (m_electricalCheckbox.IsClicked)
		{
			IndustrialMachineControl.Change(m_condenser, IndustrialControl.ElectricalControl, m_condenser.ElectricalControl ? 0 : 1);
		}
		if (m_chargeButton.IsClicked)
		{
			m_condenser.CurrentMode = ComponentCondenser.Mode.Charge;
		}
		if (m_dischargeButton.IsClicked)
		{
			m_condenser.CurrentMode = ComponentCondenser.Mode.Discharge;
		}
		m_dischargeButton.IsEnabled = !m_condenser.ElectricalControl;
		m_chargeButton.IsEnabled = !m_condenser.ElectricalControl;
		m_dischargeButton.IsChecked = m_condenser.CurrentMode == ComponentCondenser.Mode.Discharge;
		m_chargeButton.IsChecked = m_condenser.CurrentMode == ComponentCondenser.Mode.Charge;
		m_electricalCheckbox.IsChecked = m_condenser.ElectricalControl;
		m_chargeWidget.Capacitance = m_condenser.Capacitance;
		m_chargeWidget.Value = m_condenser.StoredEnergy;
		m_powerLabel.Text = string.Format(LanguageControl.GetContentWidgets("CondenserAWidget", 6), m_condenser.OutputPower.ToString("F1"));
		m_plusPowerButton.IsEnabled = m_condenser.OutputPower < ComponentCondenserA.MaxOutputPower;
		m_minusPowerButton.IsEnabled = m_condenser.OutputPower > ComponentCondenserA.MinOutputPower;
		if (m_plusPowerButton.IsClicked)
		{
			m_condenser.OutputPower += 0.1f;
			m_powerSlider.Value = m_condenser.OutputPower;
		}
		if (m_minusPowerButton.IsClicked)
		{
			m_condenser.OutputPower -= 0.1f;
			m_powerSlider.Value = m_condenser.OutputPower;
		}
		if (m_powerSlider.IsSliding)
		{
			m_condenser.OutputPower = m_powerSlider.Value;
		}
		if (!m_condenser.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
		}
	}
}
