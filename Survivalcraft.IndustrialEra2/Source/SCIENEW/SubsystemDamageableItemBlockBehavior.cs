using System.Collections.Generic;
using Engine;
using Game;
using TemplatesDatabase;

namespace SCIENEW;

public class SubsystemDamageableItemBlockBehavior : SubsystemBlockBehavior
{
	public Dictionary<string, DamageableItemBehavior[]> behaviorsMap = new Dictionary<string, DamageableItemBehavior[]>();

	public override int[] HandledBlocks => new int[1] { BlocksManager.GetBlockIndex<BaseDamageableItemBlock>() };

	public override void Load(ValuesDictionary valuesDictionary)
	{
		base.Load(valuesDictionary);
		foreach (KeyValuePair<string, int> item2 in DamageableItemManager.IdMap)
		{
			DamageableItemBlock item = DamageableItemManager.GetItem(item2.Key);
			behaviorsMap.Add(item2.Key, item.GetBehaviors(Project));
		}
	}

	public override bool OnUse(Ray3 ray, ComponentMiner componentMiner)
	{
		bool flag = false;
		string itemCraftingID = DamageableItemManager.GetItemCraftingID(componentMiner.ActiveBlockValue);
		DamageableItemBehavior[] array = behaviorsMap[itemCraftingID];
		if (array.Length != 0)
		{
			DamageableItemBehavior[] array2 = array;
			foreach (DamageableItemBehavior damageableItemBehavior in array2)
			{
				flag |= damageableItemBehavior.OnUse(ray, componentMiner);
			}
		}
		return flag;
	}

	public override bool OnHitAsProjectile(CellFace? cellFace, ComponentBody componentBody, WorldItem worldItem)
	{
		bool flag = false;
		string itemCraftingID = DamageableItemManager.GetItemCraftingID(worldItem.Value);
		DamageableItemBehavior[] array = behaviorsMap[itemCraftingID];
		if (array.Length != 0)
		{
			DamageableItemBehavior[] array2 = array;
			foreach (DamageableItemBehavior damageableItemBehavior in array2)
			{
				flag |= damageableItemBehavior.OnHitAsProjectile(cellFace, componentBody, worldItem);
			}
		}
		return flag;
	}

	public void OnMinerDig(ComponentMiner miner, TerrainRaycastResult raycastResult, ref float DigProgress, out bool Digged)
	{
		string itemCraftingID = DamageableItemManager.GetItemCraftingID(miner.ActiveBlockValue);
		bool flag = false;
		DamageableItemBehavior[] array = behaviorsMap[itemCraftingID];
		if (array.Length != 0)
		{
			DamageableItemBehavior[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].OnMinerDig(miner, raycastResult, ref DigProgress, out var Digged2);
				flag |= Digged2;
			}
		}
		Digged = flag;
	}

	public override bool OnEditBlock(int x, int y, int z, int value, ComponentPlayer componentPlayer)
	{
		bool flag = false;
		string itemCraftingID = DamageableItemManager.GetItemCraftingID(value);
		DamageableItemBehavior[] array = behaviorsMap[itemCraftingID];
		if (array.Length != 0)
		{
			DamageableItemBehavior[] array2 = array;
			foreach (DamageableItemBehavior damageableItemBehavior in array2)
			{
				flag |= damageableItemBehavior.OnEditBlock(x, y, z, value, componentPlayer);
			}
		}
		return flag;
	}

	public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer)
	{
		bool flag = false;
		string itemCraftingID = DamageableItemManager.GetItemCraftingID(inventory.GetSlotValue(slotIndex));
		DamageableItemBehavior[] array = behaviorsMap[itemCraftingID];
		if (array.Length != 0)
		{
			DamageableItemBehavior[] array2 = array;
			foreach (DamageableItemBehavior damageableItemBehavior in array2)
			{
				flag |= damageableItemBehavior.OnEditInventoryItem(inventory, slotIndex, componentPlayer);
			}
		}
		return flag;
	}
}
