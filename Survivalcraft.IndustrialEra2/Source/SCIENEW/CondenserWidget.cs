using System.Xml.Linq;
using Engine;
using Game;

namespace SCIENEW;

public class CondenserWidget : CanvasWidget
{
	public GridPanelWidget m_inventoryGrid;

	public ComponentCondenser m_condenser;

	public LabelWidget m_condenserLabel;

	public BevelledButtonWidget m_chargeButton;

	public BevelledButtonWidget m_dischargeButton;

	public ChargeWidget m_chargeWidget;

	public CheckboxWidget m_electricalCheckbox;

	public CondenserWidget(IInventory inventory, ComponentCondenser condenser)
	{
		m_condenser = condenser;
		XElement node = ContentManager.Get<XElement>("Widgets/CondenserWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_condenserLabel = Children.Find<LabelWidget>("CondenserLabel");
		m_chargeButton = Children.Find<BevelledButtonWidget>("ChargeButton");
		m_dischargeButton = Children.Find<BevelledButtonWidget>("DischargeButton");
		m_chargeWidget = Children.Find<ChargeWidget>("Charge");
		m_electricalCheckbox = Children.Find<CheckboxWidget>("ElectricalControl");
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
		if (!m_condenser.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
		}
	}
}
