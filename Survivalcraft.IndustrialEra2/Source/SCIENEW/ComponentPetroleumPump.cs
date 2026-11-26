using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentPetroleumPump : ComponentInventoryTankBase
{
	public ComponentBlockEntity m_componentBlockEntity;

	public SubsystemEnginePower m_subsystemEnginePower;

	public int m_petroleumFluidID;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemEnginePower = Project.FindSubsystem<SubsystemEnginePower>(throwOnError: true);
		m_tanks[0].Capacity = 48f;
		m_petroleumFluidID = FluidsManager.GetFluidID("PetroleumFluid", 0);
	}

	public override float GetTankCapacity(int tankIndex, int value)
	{
		if (!(FluidsManager.GetFluid(value) is PetroleumFluid))
		{
			return 0f;
		}
		return base.GetTankCapacity(tankIndex, value);
	}

	public override int GetSlotCapacity(int slotIndex, int value)
	{
		if (Terrain.ExtractContents(value) == BaseDamageableItemBlock.Index && DamageableItemManager.GetItem(value) is TubularisDItem)
		{
			return base.GetSlotCapacity(slotIndex, value);
		}
		return 0;
	}

	public void Pump(SubsystemTerrain subsystemTerrain)
	{
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		if (!m_subsystemEnginePower.IsPowered(coordinates, out IEngineComponent _))
		{
			return;
		}
		bool flag = true;
		foreach (Slot slot in m_slots)
		{
			if (slot != null && slot.Count > 0)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			return;
		}
		int innerData = DevicesUtils.GetInnerData(subsystemTerrain.Terrain.GetCellValueFast(coordinates.X, coordinates.Y, coordinates.Z));
		Point3 current2 = coordinates + CellFace.FaceToPoint3(innerData);
		HashSet<Point3> check = new HashSet<Point3>();
		Point3? point = DFS(current2, check, 40, subsystemTerrain);
		if (!point.HasValue)
		{
			return;
		}
		float volume = 4f;
		if (ComponentInventoryTankBase.FindAcquireTankForFluid(this, m_petroleumFluidID) == -1)
		{
			return;
		}
		ComponentInventoryTankBase.AcquireFluid(this, m_petroleumFluidID, volume);
		subsystemTerrain.ChangeCell(point.Value.X, point.Value.Y, point.Value.Z, 0);
		foreach (Slot slot2 in m_slots)
		{
			if (slot2 != null && slot2.Value != 0 && slot2.Count != 0)
			{
				int damage = DamageableItemManager.GetDamage(slot2.Value);
				slot2.Value = DamageableItemManager.SetDamage(slot2.Value, damage + 1);
				if (BlocksManager.Blocks[BaseDamageableItemBlock.Index].GetBlockHealth(slot2.Value) < 0f)
				{
					slot2.Count--;
				}
				break;
			}
		}
	}

	public Point3? DFS(Point3 current, HashSet<Point3> check, int maxCount, SubsystemTerrain subsystemTerrain)
	{
		if (!CheckPetroleum(current, subsystemTerrain))
		{
			return null;
		}
		if (maxCount == 0)
		{
			return current;
		}
		check.Add(current);
		for (int i = 0; i < 6; i++)
		{
			Point3 point = current + CellFace.FaceToPoint3(i);
			if (CheckPetroleum(point, subsystemTerrain) && !check.Contains(point))
			{
				return DFS(point, check, maxCount - 1, subsystemTerrain);
			}
		}
		return current;
	}

	public bool CheckPetroleum(Point3 current, SubsystemTerrain subsystemTerrain)
	{
		int cellValue = subsystemTerrain.Terrain.GetCellValue(current.X, current.Y, current.Z);
		int num = Terrain.ExtractContents(cellValue);
		int data = Terrain.ExtractData(cellValue);
		if (num != PetroleumBlock.Index || FluidBlock.GetLevel(data) != 0)
		{
			return false;
		}
		return true;
	}
}
