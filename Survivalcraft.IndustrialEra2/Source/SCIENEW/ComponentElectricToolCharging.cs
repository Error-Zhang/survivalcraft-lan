using System;
using Game;

namespace SCIENEW;

public class ComponentElectricToolCharging : ComponentInventoryBase
{
    internal int GetTransferAmount(IInventory inventory, int slot)
    {
        if ((uint)slot >= inventory.SlotsCount || inventory.GetSlotCount(slot) <= 0 || GetSlotCount(0) <= 0) return 0;
        int tool = inventory.GetSlotValue(slot);
        int battery = GetSlotValue(0);
        if (Terrain.ExtractContents(tool) != BaseDamageableItemBlock.Index || Terrain.ExtractContents(battery) != BaseDamageableItemBlock.Index
            || DamageableItemManager.GetItem(tool) is not IChargeableDItem chargeable
            || DamageableItemManager.GetItem(battery) is not BatteryDItem powerSource) return 0;

        return Math.Max(0, Math.Min(chargeable.GetDamage(tool), powerSource.GetDurability(battery) - powerSource.GetDamage(battery)));
    }

    internal bool TryCharge(IInventory inventory, int slot)
    {
        int amount = GetTransferAmount(inventory, slot);
        if (amount <= 0) return false;
        int tool = inventory.GetSlotValue(slot);
        int battery = GetSlotValue(0);
        int charged = ((IChargeableDItem)DamageableItemManager.GetItem(tool)).Charge(tool, amount);
        int spent = ((BatteryDItem)DamageableItemManager.GetItem(battery)).Discharge(battery, amount);
        inventory.RemoveSlotItems(slot, 1);
        inventory.AddSlotItems(slot, charged, 1);
        RemoveSlotItems(0, 1);
        AddSlotItems(0, spent, 1);
        return true;
    }

    internal void DropBattery(ComponentPlayer player)
    {
        DropSlotItems(this, 0, player.ComponentBody.Position, Engine.Vector3.Zero);
    }
}
