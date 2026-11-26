using System.Xml.Linq;
using Engine;
using Game;

namespace SCIENEW;

public class ChargerWidget : CanvasWidget
{
	public GridPanelWidget m_inventoryGrid;

	public GridPanelWidget m_chargerGrid;

	public ComponentCharger m_charger;

	public BevelledButtonWidget m_chargeButton;

	public BevelledButtonWidget m_dischargeButton;

	public CheckboxWidget m_electricalCheckbox;

	public ChargerWidget(IInventory inventory, ComponentCharger charger)
	{
		m_charger = charger;
		XElement node = ContentManager.Get<XElement>("Widgets/ChargerWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_chargeButton = Children.Find<BevelledButtonWidget>("ChargeButton");
		m_dischargeButton = Children.Find<BevelledButtonWidget>("DischargeButton");
		m_chargerGrid = Children.Find<GridPanelWidget>("ChargerGrid");
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
		num = 0;
		for (int k = 0; k < m_chargerGrid.RowsCount; k++)
		{
			for (int l = 0; l < m_chargerGrid.ColumnsCount; l++)
			{
				InventorySlotWidget inventorySlotWidget2 = new InventorySlotWidget();
				inventorySlotWidget2.AssignInventorySlot(charger, num++);
				m_chargerGrid.Children.Add(inventorySlotWidget2);
				m_chargerGrid.SetWidgetCell(inventorySlotWidget2, new Point2(l, k));
			}
		}
	}

	public override void Update()
	{
		if (m_electricalCheckbox.IsClicked)
		{
			IndustrialMachineControl.Change(m_charger, IndustrialControl.ElectricalControl, m_charger.ElectricalControl ? 0 : 1);
		}
		if (m_chargeButton.IsClicked)
		{
			IndustrialMachineControl.Change(m_charger, IndustrialControl.OperatingMode, 0);
		}
		if (m_dischargeButton.IsClicked)
		{
			IndustrialMachineControl.Change(m_charger, IndustrialControl.OperatingMode, 1);
		}
		m_dischargeButton.IsEnabled = !m_charger.ElectricalControl;
		m_chargeButton.IsEnabled = !m_charger.ElectricalControl;
		m_dischargeButton.IsChecked = m_charger.Mode == ComponentCondenser.Mode.Discharge;
		m_chargeButton.IsChecked = m_charger.Mode == ComponentCondenser.Mode.Charge;
		m_electricalCheckbox.IsChecked = m_charger.ElectricalControl;
		if (!m_charger.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
		}
	}
}
