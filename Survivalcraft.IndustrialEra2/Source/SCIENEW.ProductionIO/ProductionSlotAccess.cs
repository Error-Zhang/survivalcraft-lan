using System;
using System.Collections.Generic;
using Game;
using GameEntitySystem;
using ZLinq;
using ZLinq.Linq;

namespace SCIENEW.ProductionIO;

public static class ProductionSlotAccess
{
	public static IEnumerable<int> GetInventorySourceSlotOrder(Entity sourceEntity, IInventory inventory, int filterValue, int explicitOneBasedSlot)
	{
		return GetInventorySlotOrder(sourceEntity, inventory, ProductionSlotKind.Output, filterValue, explicitOneBasedSlot);
	}

	public static bool TryInsertIntoInputSlots(Entity destEntity, IInventory destInventory, int itemValue, int itemCount, int explicitOneBasedSlot)
	{
		if (explicitOneBasedSlot > 0)
		{
			int slotIndex = explicitOneBasedSlot - 1;
			if (destInventory.SlotsCount < explicitOneBasedSlot)
			{
				return false;
			}
			return TryInsertInventorySlot(destInventory, slotIndex, itemValue, itemCount);
		}
		IInventoryProductionSlots inventoryProductionSlots = ResolveInventorySlots(destEntity, destInventory);
		if (inventoryProductionSlots != null)
		{
			ValueEnumerator<FromEnumerable<int>, int> enumerator = inventoryProductionSlots.GetSlotIndices(ProductionSlotKind.Input).AsValueEnumerable().GetEnumerator<FromEnumerable<int>, int>();
			try
			{
				while (enumerator.MoveNext())
				{
					int current = enumerator.Current;
					if (current >= 0 && current < destInventory.SlotsCount && TryInsertInventorySlot(destInventory, current, itemValue, itemCount))
					{
						return true;
					}
				}
			}
			finally
			{
				enumerator.Dispose();
			}
		}
		return ComponentInventoryTankBase.AcquireItems(destInventory, itemValue, itemCount, 0..destInventory.SlotsCount) < 1;
	}

	public static IEnumerable<int> GetTankSourceSlotOrder(ITank fromTank, int filterValue, int explicitOneBasedIndex)
	{
		if (explicitOneBasedIndex > 0)
		{
			return new _003C_003Ez__ReadOnlySingleElementList<int>(explicitOneBasedIndex - 1);
		}
		if (fromTank is ITankProductionSlots tankProductionSlots)
		{
			int[] array = CollectValidIndices(tankProductionSlots.GetSlotIndices(ProductionSlotKind.Output), fromTank.TanksCount);
			if (array.Length != 0 && HasTankCandidate(fromTank, array, filterValue))
			{
				return array;
			}
		}
		return BuildSequentialIndices(fromTank.TanksCount);
	}

	public static bool CanInsertFluidIntoInputTanks(ITank toTank, int value, int explicitOneBasedIndex)
	{
		if (explicitOneBasedIndex > 0)
		{
			int num = explicitOneBasedIndex - 1;
			if (num >= toTank.TanksCount)
			{
				return false;
			}
			if (toTank.GetTankVolume(num) == 0f)
			{
				return toTank.GetTankCapacity(num, value) > 0f;
			}
			if (toTank.GetTankValue(num) != value)
			{
				return false;
			}
			return toTank.GetTankVolume(num) < toTank.GetTankCapacity(num, value);
		}
		if (toTank is ITankProductionSlots tankProductionSlots)
		{
			ValueEnumerator<FromEnumerable<int>, int> enumerator = tankProductionSlots.GetSlotIndices(ProductionSlotKind.Input).AsValueEnumerable().GetEnumerator<FromEnumerable<int>, int>();
			try
			{
				while (enumerator.MoveNext())
				{
					int current = enumerator.Current;
					if (current >= 0 && current < toTank.TanksCount && ComponentInventoryTankBase.FindAcquireTankForFluid(toTank, value, current..(current + 1)) >= 0)
					{
						return true;
					}
				}
			}
			finally
			{
				enumerator.Dispose();
			}
		}
		return ComponentInventoryTankBase.FindAcquireTankForFluid(toTank, value) >= 0;
	}

	public static float InsertFluidIntoInputTanks(ITank toTank, int value, float volume, int explicitOneBasedIndex)
	{
		if (explicitOneBasedIndex > 0)
		{
			int tankIndex = explicitOneBasedIndex - 1;
			if (toTank.GetTankValue(tankIndex) == value || toTank.GetTankVolume(tankIndex) == 0f)
			{
				toTank.AddTankFluid(tankIndex, value, volume, out var overflow);
				return overflow;
			}
			return volume;
		}
		if (toTank is ITankProductionSlots tankProductionSlots)
		{
			ValueEnumerator<FromEnumerable<int>, int> enumerator = tankProductionSlots.GetSlotIndices(ProductionSlotKind.Input).AsValueEnumerable().GetEnumerator<FromEnumerable<int>, int>();
			try
			{
				while (enumerator.MoveNext())
				{
					int current = enumerator.Current;
					if (current >= 0 && current < toTank.TanksCount)
					{
						float num = ComponentInventoryTankBase.AcquireFluid(toTank, value, volume, current..(current + 1));
						if (num < volume)
						{
							return num;
						}
					}
				}
			}
			finally
			{
				enumerator.Dispose();
			}
		}
		return ComponentInventoryTankBase.AcquireFluid(toTank, value, volume);
	}

	private static IEnumerable<int> GetInventorySlotOrder(Entity sourceEntity, IInventory inventory, ProductionSlotKind preferredKind, int filterValue, int explicitOneBasedSlot)
	{
		if (explicitOneBasedSlot > 0)
		{
			return new _003C_003Ez__ReadOnlySingleElementList<int>(explicitOneBasedSlot - 1);
		}
		IInventoryProductionSlots inventoryProductionSlots = ResolveInventorySlots(sourceEntity, inventory);
		if (inventoryProductionSlots != null)
		{
			int[] array = CollectValidIndices(inventoryProductionSlots.GetSlotIndices(preferredKind), inventory.SlotsCount);
			if (array.Length != 0 && HasInventoryCandidate(inventory, array, filterValue))
			{
				return array;
			}
		}
		return BuildSequentialIndices(inventory.SlotsCount);
	}

	private static int[] CollectValidIndices(IEnumerable<int> slotIndices, int count)
	{
		int[] array = new int[count];
		int num = 0;
		ValueEnumerator<FromEnumerable<int>, int> enumerator = slotIndices.AsValueEnumerable().GetEnumerator<FromEnumerable<int>, int>();
		try
		{
			while (enumerator.MoveNext())
			{
				int current = enumerator.Current;
				if (current >= 0 && current < count)
				{
					array[num++] = current;
				}
			}
		}
		finally
		{
			enumerator.Dispose();
		}
		if (num == array.Length)
		{
			return array;
		}
		int[] array2 = new int[num];
		Array.Copy(array, array2, num);
		return array2;
	}

	private static int[] BuildSequentialIndices(int count)
	{
		int[] array = new int[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = i;
		}
		return array;
	}

	private static bool HasInventoryCandidate(IInventory inventory, int[] slots, int filterValue)
	{
		foreach (int slotIndex in slots)
		{
			if (inventory.GetSlotCount(slotIndex) > 0 && (filterValue == 0 || inventory.GetSlotValue(slotIndex) == filterValue))
			{
				return true;
			}
		}
		return false;
	}

	private static bool HasTankCandidate(ITank tank, int[] slots, int filterValue)
	{
		foreach (int tankIndex in slots)
		{
			if (!(tank.GetTankVolume(tankIndex) <= 0f))
			{
				int tankValue = tank.GetTankValue(tankIndex);
				if (filterValue == 0 || tankValue == filterValue)
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool TryInsertInventorySlot(IInventory inventory, int slotIndex, int itemValue, int itemCount)
	{
		if (inventory.GetSlotCount(slotIndex) == 0)
		{
			if (inventory.GetSlotCapacity(slotIndex, itemValue) < itemCount)
			{
				return false;
			}
			inventory.AddSlotItems(slotIndex, itemValue, itemCount);
			return true;
		}
		if (inventory.GetSlotValue(slotIndex) != itemValue)
		{
			return false;
		}
		if (inventory.GetSlotCount(slotIndex) + itemCount > inventory.GetSlotCapacity(slotIndex, itemValue))
		{
			return false;
		}
		inventory.AddSlotItems(slotIndex, itemValue, itemCount);
		return true;
	}

	private static IInventoryProductionSlots? ResolveInventorySlots(Entity entity, IInventory inventory)
	{
		return (inventory as IInventoryProductionSlots) ?? entity.FindComponent<IInventoryProductionSlots>();
	}
}
