using System.Collections.Generic;
using Engine;
using Game;
using TemplatesDatabase;

namespace SCIENEW;

public class SubsystemDeviceBlockBehavior : SubsystemPollableBlockBehavior
{
	public Dictionary<string, DeviceBehavior[]> behaviorsMap = new Dictionary<string, DeviceBehavior[]>();

	public override int[] HandledBlocks => new int[1] { BlocksManager.GetBlockIndex<BaseDeviceBlock>() };

	public override void Load(ValuesDictionary valuesDictionary)
	{
		base.Load(valuesDictionary);
		foreach (KeyValuePair<string, int[]> item in DevicesBlockManager.IdMap)
		{
			DeviceBlock device = DevicesBlockManager.GetDevice(item.Key);
			behaviorsMap.Add(item.Key, device.GetBehaviors(Project));
		}
	}

	public override void OnBlockAdded(int value, int oldValue, int x, int y, int z)
	{
		string craftingId = DevicesBlockManager.GetDevice(value).GetCraftingId();
		DeviceBehavior[] array = behaviorsMap[craftingId];
		if (array.Length != 0)
		{
			DeviceBehavior[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].OnBlockAdded(value, oldValue, x, y, z);
			}
		}
	}

	public override void OnBlockRemoved(int value, int newValue, int x, int y, int z)
	{
		string craftingId = DevicesBlockManager.GetDevice(value).GetCraftingId();
		DeviceBehavior[] array = behaviorsMap[craftingId];
		if (array.Length != 0)
		{
			DeviceBehavior[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].OnBlockRemoved(value, newValue, x, y, z);
			}
		}
	}

	public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
	{
		bool flag = false;
		string craftingId = DevicesBlockManager.GetDevice(raycastResult.Value).GetCraftingId();
		DeviceBehavior[] array = behaviorsMap[craftingId];
		if (array.Length != 0)
		{
			DeviceBehavior[] array2 = array;
			foreach (DeviceBehavior deviceBehavior in array2)
			{
				flag |= deviceBehavior.OnInteract(raycastResult, componentMiner);
			}
		}
		return flag;
	}

	public override bool OnUse(Ray3 ray, ComponentMiner componentMiner)
	{
		bool flag = false;
		string craftingId = DevicesBlockManager.GetDevice(componentMiner.ActiveBlockValue).GetCraftingId();
		DeviceBehavior[] array = behaviorsMap[craftingId];
		if (array.Length != 0)
		{
			DeviceBehavior[] array2 = array;
			foreach (DeviceBehavior deviceBehavior in array2)
			{
				flag |= deviceBehavior.OnUse(ray, componentMiner);
			}
		}
		return flag;
	}

	public override void OnBlockGenerated(int value, int x, int y, int z, bool isLoaded)
	{
		string craftingId = DevicesBlockManager.GetDevice(value).GetCraftingId();
		DeviceBehavior[] array = behaviorsMap[craftingId];
		if (array.Length != 0)
		{
			DeviceBehavior[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].OnBlockGenerated(value, x, y, z, isLoaded);
			}
		}
	}

	public override void OnBlockModified(int value, int oldValue, int x, int y, int z)
	{
		DeviceBlock device = DevicesBlockManager.GetDevice(oldValue);
		DeviceBlock device2 = DevicesBlockManager.GetDevice(value);
		if (device == device2)
		{
			string craftingId = device2.GetCraftingId();
			DeviceBehavior[] array = behaviorsMap[craftingId];
			if (array.Length != 0)
			{
				DeviceBehavior[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					array2[i].OnBlockModified(value, oldValue, x, y, z);
				}
			}
			return;
		}
		string craftingId2 = device.GetCraftingId();
		string craftingId3 = device2.GetCraftingId();
		DeviceBehavior[] array3 = behaviorsMap[craftingId2];
		DeviceBehavior[] array4 = behaviorsMap[craftingId3];
		if (array3.Length != 0)
		{
			DeviceBehavior[] array2 = array3;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].OnBlockRemoved(oldValue, value, x, y, z);
			}
		}
		if (array4.Length != 0)
		{
			DeviceBehavior[] array2 = array4;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].OnBlockAdded(value, oldValue, x, y, z);
			}
		}
	}

	public override void OnNeighborBlockChanged(int x, int y, int z, int neighborX, int neighborY, int neighborZ)
	{
		string craftingId = DevicesBlockManager.GetDevice(SubsystemTerrain.Terrain.GetCellValueFast(x, y, z)).GetCraftingId();
		DeviceBehavior[] array = behaviorsMap[craftingId];
		if (array.Length != 0)
		{
			DeviceBehavior[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].OnNeighborBlockChanged(x, y, z, neighborX, neighborY, neighborZ);
			}
		}
	}

	public override void OnHitByProjectile(CellFace cellFace, WorldItem worldItem)
	{
		string craftingId = DevicesBlockManager.GetDevice(SubsystemTerrain.Terrain.GetCellValueFast(cellFace.X, cellFace.Y, cellFace.Z)).GetCraftingId();
		DeviceBehavior[] array = behaviorsMap[craftingId];
		if (array.Length != 0)
		{
			DeviceBehavior[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].OnHitByProjectile(cellFace, worldItem);
			}
		}
	}

	public override void OnCollide(CellFace cellFace, float velocity, ComponentBody componentBody)
	{
		string craftingId = DevicesBlockManager.GetDevice(SubsystemTerrain.Terrain.GetCellValueFast(cellFace.X, cellFace.Y, cellFace.Z)).GetCraftingId();
		DeviceBehavior[] array = behaviorsMap[craftingId];
		if (array.Length != 0)
		{
			DeviceBehavior[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].OnCollide(cellFace, velocity, componentBody);
			}
		}
	}

	public override void OnPoll(int value, int x, int y, int z, int pollPass)
	{
		string craftingId = DevicesBlockManager.GetDevice(value).GetCraftingId();
		DeviceBehavior[] array = behaviorsMap[craftingId];
		if (array.Length != 0)
		{
			DeviceBehavior[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].OnPoll(value, x, y, z, pollPass);
			}
		}
	}
}
