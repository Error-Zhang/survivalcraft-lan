using Game;
using GameEntitySystem;

namespace SCIENEW;

public class ChargeByBatteryDItemBehavior : DamageableItemBehavior
{
	public ChargeByBatteryDItemBehavior(Project project)
		: base(project)
	{
	}

	public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer)
	{
		ComponentElectricToolCharging componentElectricToolCharging = componentPlayer.Entity.FindComponent<ComponentElectricToolCharging>(throwOnError: true);
		componentPlayer.ComponentGui.ModalPanelWidget = ((componentPlayer.ComponentGui.ModalPanelWidget == null) ? new ElectricToolChargeWidget(inventory, slotIndex, componentElectricToolCharging, inventory.GetSlotValue(slotIndex)) : null);
		return true;
	}
}
