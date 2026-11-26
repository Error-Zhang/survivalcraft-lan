using Engine;
using Game;
using GameEntitySystem;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentLiquidPump : ComponentInventoryTankBase, IUpdateable
{
	public ComponentBlockEntity m_componentBlockEntity;

	public SubsystemEnginePower m_subsystemEnginePower;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public override int GetSlotCapacity(int slotIndex, int value)
	{
		if (Terrain.ExtractContents(value) == BaseDamageableItemBlock.Index && DamageableItemManager.GetItem(value) is TubularisDItem)
		{
			return base.GetSlotCapacity(slotIndex, value);
		}
		return 0;
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemEnginePower = Project.FindSubsystem<SubsystemEnginePower>(throwOnError: true);
		m_tanks[0].Capacity = (m_tanks[1].Capacity = (m_tanks[2].Capacity = (m_tanks[3].Capacity = 12f)));
	}

	public void Update(float dt)
	{
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
		Vector3 vector = -CellFace.FaceToVector3(DevicesUtils.GetSixFaceDirection(subsystemTerrain.Terrain.GetCellValueFast(coordinates.X, coordinates.Y, coordinates.Z)));
		int x = coordinates.X;
		int y = coordinates.Y;
		int z = coordinates.Z;
		int[] array = new int[25]
		{
			0, 0, 1, 1, 1, 0, -1, -1, -1, -1,
			0, 1, 2, 2, 2, 2, 2, 1, 0, -1,
			-2, -2, -2, -2, -2
		};
		int[] array2 = new int[25]
		{
			0, -1, -1, 0, 1, 1, 1, 0, -1, -2,
			-2, -2, -2, -1, 0, 1, 2, 2, 2, 2,
			2, 1, 0, -1, -2
		};
		for (int i = 1; i < 8; i++)
		{
			for (int j = 0; j < 25; j++)
			{
				x = coordinates.X - (int)vector.X * i;
				y = coordinates.Y - (int)vector.Y * i;
				z = coordinates.Z - (int)vector.Z * i;
				if (vector.X != 0f)
				{
					y = coordinates.Y + array[j];
					z = coordinates.Z + array2[j];
				}
				if (vector.Y != 0f)
				{
					x = coordinates.X + array[j];
					z = coordinates.Z + array2[j];
				}
				if (vector.Z != 0f)
				{
					x = coordinates.X + array[j];
					y = coordinates.Y + array2[j];
				}
				int num = Terrain.ReplaceLight(subsystemTerrain.Terrain.GetCellValue(x, y, z), 0);
				int num2 = Terrain.ExtractContents(num);
				if ((!FluidsManager.FluidToBlock.ContainsValue(num) && !FluidsManager.FluidToBlock.ContainsValue(num2)) || !(BlocksManager.Blocks[num2] is FluidBlock) || FluidBlock.GetLevel(Terrain.ExtractData(num)) != 0)
				{
					continue;
				}
				int value = (FluidsManager.FluidToBlock.ContainsValue(num) ? FluidsManager.FluidToBlock.GetBySecond(num) : FluidsManager.FluidToBlock.GetBySecond(num2));
				float volume = 4f;
				if (ComponentInventoryTankBase.FindAcquireTankForFluid(this, value) == -1)
				{
					continue;
				}
				ComponentInventoryTankBase.AcquireFluid(this, value, volume);
				subsystemTerrain.ChangeCell(x, y, z, 0);
				{
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
					return;
				}
			}
		}
	}
}
