using System.Collections.Generic;
using Engine;
using Game;
using TemplatesDatabase;

namespace SCIENEW;

public class SubsystemNormalBlockBehavior : SubsystemPollableBlockBehavior, IUpdateable
{
	public Dictionary<string, NormalBlockBehavior[]> behaviorsMap = new Dictionary<string, NormalBlockBehavior[]>();

	public override int[] HandledBlocks => new int[1] { BlocksManager.GetBlockIndex<BaseNormalBlock>() };

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public override void Load(ValuesDictionary valuesDictionary)
	{
		base.Load(valuesDictionary);
		foreach (KeyValuePair<string, int> item in NormalBlockManager.IdMap)
		{
			NormalBlock normalBlock = NormalBlockManager.GetNormalBlock(item.Key);
			behaviorsMap.Add(item.Key, normalBlock.GetBehaviors(Project));
		}
	}

	public override void OnBlockAdded(int value, int oldValue, int x, int y, int z)
	{
		string craftingId = NormalBlockManager.GetNormalBlock(value).GetCraftingId();
		NormalBlockBehavior[] array = behaviorsMap[craftingId];
		if (array.Length != 0)
		{
			NormalBlockBehavior[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].OnBlockAdded(value, oldValue, x, y, z);
			}
		}
	}

	public override void OnBlockRemoved(int value, int newValue, int x, int y, int z)
	{
		string craftingId = NormalBlockManager.GetNormalBlock(value).GetCraftingId();
		NormalBlockBehavior[] array = behaviorsMap[craftingId];
		if (array.Length != 0)
		{
			NormalBlockBehavior[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].OnBlockRemoved(value, newValue, x, y, z);
			}
		}
	}

	public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
	{
		bool flag = false;
		string craftingId = NormalBlockManager.GetNormalBlock(raycastResult.Value).GetCraftingId();
		NormalBlockBehavior[] array = behaviorsMap[craftingId];
		if (array.Length != 0)
		{
			NormalBlockBehavior[] array2 = array;
			foreach (NormalBlockBehavior normalBlockBehavior in array2)
			{
				flag |= normalBlockBehavior.OnInteract(raycastResult, componentMiner);
			}
		}
		return flag;
	}

	public override bool OnUse(Ray3 ray, ComponentMiner componentMiner)
	{
		bool flag = false;
		string craftingId = NormalBlockManager.GetNormalBlock(componentMiner.ActiveBlockValue).GetCraftingId();
		NormalBlockBehavior[] array = behaviorsMap[craftingId];
		if (array.Length != 0)
		{
			NormalBlockBehavior[] array2 = array;
			foreach (NormalBlockBehavior normalBlockBehavior in array2)
			{
				flag |= normalBlockBehavior.OnUse(ray, componentMiner);
			}
		}
		return flag;
	}

	public override void OnBlockGenerated(int value, int x, int y, int z, bool isLoaded)
	{
		string craftingId = NormalBlockManager.GetNormalBlock(value).GetCraftingId();
		NormalBlockBehavior[] array = behaviorsMap[craftingId];
		if (array.Length != 0)
		{
			NormalBlockBehavior[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].OnBlockGenerated(value, x, y, z, isLoaded);
			}
		}
	}

	public override void OnBlockModified(int value, int oldValue, int x, int y, int z)
	{
		string craftingId = NormalBlockManager.GetNormalBlock(value).GetCraftingId();
		NormalBlockBehavior[] array = behaviorsMap[craftingId];
		if (array.Length != 0)
		{
			NormalBlockBehavior[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].OnBlockModified(value, oldValue, x, y, z);
			}
		}
	}

	public override void OnNeighborBlockChanged(int x, int y, int z, int neighborX, int neighborY, int neighborZ)
	{
		string craftingId = NormalBlockManager.GetNormalBlock(SubsystemTerrain.Terrain.GetCellValueFast(x, y, z)).GetCraftingId();
		NormalBlockBehavior[] array = behaviorsMap[craftingId];
		if (array.Length != 0)
		{
			NormalBlockBehavior[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].OnNeighborBlockChanged(x, y, z, neighborX, neighborY, neighborZ);
			}
		}
	}

	public override void OnHitByProjectile(CellFace cellFace, WorldItem worldItem)
	{
		string craftingId = NormalBlockManager.GetNormalBlock(SubsystemTerrain.Terrain.GetCellValueFast(cellFace.X, cellFace.Y, cellFace.Z)).GetCraftingId();
		NormalBlockBehavior[] array = behaviorsMap[craftingId];
		if (array.Length != 0)
		{
			NormalBlockBehavior[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].OnHitByProjectile(cellFace, worldItem);
			}
		}
	}

	public override void OnCollide(CellFace cellFace, float velocity, ComponentBody componentBody)
	{
		string craftingId = NormalBlockManager.GetNormalBlock(SubsystemTerrain.Terrain.GetCellValueFast(cellFace.X, cellFace.Y, cellFace.Z)).GetCraftingId();
		NormalBlockBehavior[] array = behaviorsMap[craftingId];
		if (array.Length != 0)
		{
			NormalBlockBehavior[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].OnCollide(cellFace, velocity, componentBody);
			}
		}
	}

	public override void OnPoll(int value, int x, int y, int z, int pollPass)
	{
		string craftingId = NormalBlockManager.GetNormalBlock(SubsystemTerrain.Terrain.GetCellValueFast(x, y, z)).GetCraftingId();
		NormalBlockBehavior[] array = behaviorsMap[craftingId];
		if (array.Length != 0)
		{
			NormalBlockBehavior[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].OnPoll(value, x, y, z, pollPass);
			}
		}
	}

	public void Update(float dt)
	{
		foreach (KeyValuePair<string, NormalBlockBehavior[]> item in behaviorsMap)
		{
			if (item.Value.Length == 0)
			{
				continue;
			}
			NormalBlockBehavior[] value = item.Value;
			for (int i = 0; i < value.Length; i++)
			{
				if (value[i] is IUpdateable updateable)
				{
					updateable.Update(dt);
				}
			}
		}
	}
}
