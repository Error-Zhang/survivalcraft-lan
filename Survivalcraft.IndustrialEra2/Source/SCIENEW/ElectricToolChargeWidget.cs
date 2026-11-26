using System.Xml.Linq;
using Engine;
using Game;

namespace SCIENEW;

public class ElectricToolChargeWidget : CanvasWidget, IRemovedActionWidget
{
	public GridPanelWidget m_inventoryGrid;

	public InventorySlotWidget m_batterySlot;

	public InventorySlotWidget m_electricToolSlot;

	public ComponentElectricToolCharging m_componentElectricToolCharging;

	public int m_chargingToolIndex;

	public int m_chargingSlotIndex;

	public IInventory m_inventory;

	private double m_nextChargeRequest;

	public ElectricToolChargeWidget(IInventory inventory, int slotIndex, ComponentElectricToolCharging componentElectricToolCharging, int chargingToolIndex)
	{
		m_inventory = inventory;
		m_chargingSlotIndex = slotIndex;
		m_componentElectricToolCharging = componentElectricToolCharging;
		m_chargingToolIndex = chargingToolIndex;
		XElement node = ContentManager.Get<XElement>("Widgets/ElectricToolChargeWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_batterySlot = Children.Find<InventorySlotWidget>("BatterySlot");
		m_electricToolSlot = Children.Find<InventorySlotWidget>("ElectricToolSlot");
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
		m_electricToolSlot.AssignInventorySlot(inventory, slotIndex);
		m_batterySlot.AssignInventorySlot(componentElectricToolCharging, 0);
	}

	public override void Update()
	{
		int slotValue = m_inventory.GetSlotValue(m_chargingSlotIndex);
		if (Terrain.ExtractContents(slotValue) != BaseDamageableItemBlock.Index
			|| DamageableItemManager.GetItem(slotValue) != DamageableItemManager.GetItem(m_chargingToolIndex))
		{
			ParentWidget.Children.Remove(this);
			return;
		}
		if (!IndustrialEraLanAdapter.IsRemote(m_componentElectricToolCharging.Project))
		{
			m_componentElectricToolCharging.TryCharge(m_inventory, m_chargingSlotIndex);
		}
		else if (Time.RealTime >= m_nextChargeRequest && m_componentElectricToolCharging.GetTransferAmount(m_inventory, m_chargingSlotIndex) > 0)
		{
			m_nextChargeRequest = Time.RealTime + 0.25;
			ComponentPlayer player = m_componentElectricToolCharging.Entity.FindComponent<ComponentPlayer>(throwOnError: true);
			IndustrialEraLanAdapter.QueueToolCharge(player, m_chargingSlotIndex, slotValue);
		}
	}

	public void OnRemoved()
	{
		ComponentPlayer player = m_componentElectricToolCharging.Entity.FindComponent<ComponentPlayer>(throwOnError: true);
		if (IndustrialEraLanAdapter.IsRemote(player.Project))
		{
			IndustrialEraLanAdapter.QueueToolCharge(player, -1, 0);
		}
		else
		{
			m_componentElectricToolCharging.DropBattery(player);
		}
	}
}
