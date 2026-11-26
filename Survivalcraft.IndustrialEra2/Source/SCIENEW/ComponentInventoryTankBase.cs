using System;
using System.Collections.Generic;
using System.Globalization;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions;
using TemplatesDatabase;

namespace SCIENEW;

public abstract class ComponentInventoryTankBase : ComponentInventoryBase, ITank
{
	public class Tank
	{
		public int Value;

		public float Volume;

		public float Capacity = 4f;
	}

	public List<Tank> m_tanks = new List<Tank>();

	Project ITank.Project => Project;

	public virtual int TanksCount => m_tanks.Count;

	public int VisibleTanksCount
	{
		get
		{
			return TanksCount;
		}
		set
		{
		}
	}

	public static int FindAcquireTankForFluid(ITank tank, int value)
	{
		return ComponentTankBase.FindAcquireTankForFluid(tank, value);
	}

	public static float AcquireFluid(ITank tank, int value, float volume)
	{
		return ComponentTankBase.AcquireFluid(tank, value, volume);
	}

	public static int FindAcquireTankForFluid(ITank tank, int value, Range tankRange)
	{
		return ComponentTankBase.FindAcquireTankForFluid(tank, value, tankRange);
	}

	public static float AcquireFluid(ITank tank, int value, float volume, Range tankRange)
	{
		return ComponentTankBase.AcquireFluid(tank, value, volume, tankRange);
	}

	public static void EmitTankFluid(ITank tank, int tankIndex, Vector3 position, Vector3 velocity)
	{
		ComponentTankBase.EmitTankFluid(tank, tankIndex, position, velocity);
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		int value = valuesDictionary.GetValue<int>("TanksCount");
		for (int i = 0; i < value; i++)
		{
			m_tanks.Add(new Tank());
		}
		ValuesDictionary value2 = valuesDictionary.GetValue<ValuesDictionary>("Tanks");
		for (int j = 0; j < m_tanks.Count; j++)
		{
			ValuesDictionary value3 = value2.GetValue<ValuesDictionary>("Tank" + j.ToString(CultureInfo.InvariantCulture), null);
			if (value3 != null)
			{
				Tank tank = m_tanks[j];
				tank.Value = value3.GetValue<int>("Contents");
				tank.Volume = value3.GetValue<float>("Volume");
			}
		}
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
		ValuesDictionary valuesDictionary2 = new ValuesDictionary();
		valuesDictionary.SetValue("Tanks", valuesDictionary2);
		for (int i = 0; i < m_tanks.Count; i++)
		{
			Tank tank = m_tanks[i];
			if (tank.Volume > 0f)
			{
				ValuesDictionary valuesDictionary3 = new ValuesDictionary();
				valuesDictionary2.SetValue("Tank" + i.ToString(CultureInfo.InvariantCulture), valuesDictionary3);
				valuesDictionary3.SetValue("Contents", tank.Value);
				valuesDictionary3.SetValue("Volume", tank.Volume);
			}
		}
	}

	public virtual int GetTankValue(int tankIndex)
	{
		if (tankIndex >= 0 && tankIndex < m_tanks.Count)
		{
			if (m_tanks[tankIndex].Volume <= 0f)
			{
				return 0;
			}
			return m_tanks[tankIndex].Value;
		}
		return 0;
	}

	public virtual float GetTankVolume(int tankIndex)
	{
		if (tankIndex >= 0 && tankIndex < m_tanks.Count)
		{
			return m_tanks[tankIndex].Volume;
		}
		return 0f;
	}

	public virtual float GetTankCapacity(int tankIndex, int value)
	{
		return GetTankOriginalCapacity(tankIndex);
	}

	public float GetTankOriginalCapacity(int tankIndex)
	{
		if (tankIndex >= 0 && tankIndex < m_tanks.Count)
		{
			return m_tanks[tankIndex].Capacity;
		}
		return 0f;
	}

	public virtual bool GetTankScoopable(int tankIndex, int container)
	{
		if (FluidsManager.GetFluid(GetTankValue(tankIndex)) is IScoopableFluid scoopableFluid)
		{
			int bucketIndex;
			return scoopableFluid.IsScoopable(container, out bucketIndex);
		}
		return false;
	}

	public virtual void AddTankFluid(int tankIndex, int value, float volume, out float overflow)
	{
		overflow = 0f;
		if (volume > 0f && tankIndex >= 0 && tankIndex < m_tanks.Count)
		{
			Tank tank = m_tanks[tankIndex];
			if (GetTankVolume(tankIndex) != 0f && GetTankValue(tankIndex) != value)
			{
				throw new InvalidOperationException("Cannot add slot items.");
			}
			overflow = GetTankVolume(tankIndex) + volume - GetTankCapacity(tankIndex, value);
			tank.Value = value;
			tank.Volume = ((overflow > 0f) ? tank.Capacity : (tank.Volume + volume));
		}
	}

	public virtual float RemoveTankFluid(int tankIndex, float volume)
	{
		if (tankIndex >= 0 && tankIndex < m_tanks.Count)
		{
			Tank tank = m_tanks[tankIndex];
			volume = MathUtils.Min(volume, GetTankVolume(tankIndex));
			tank.Volume -= volume;
			return volume;
		}
		return 0f;
	}

	public virtual void EmitAllFluids(Vector3 position)
	{
		for (int i = 0; i < TanksCount; i++)
		{
			EmitTankFluid(this, i, position, m_random.Float(5f, 10f) * new Vector3(m_random.Float(-1f, 1f), m_random.Float(1f, 2f), m_random.Float(-1f, 1f)).Normalize());
		}
	}

	public static int FindAcquireSlotForItem(IInventory inventory, int value, Range slotRange)
	{
		int num = Math.Clamp(slotRange.Start.Value, 0, inventory.SlotsCount);
		int num2 = Math.Clamp(slotRange.End.Value, 0, inventory.SlotsCount);
		for (int i = num; i < num2; i++)
		{
			if (inventory.GetSlotCount(i) > 0 && inventory.GetSlotValue(i) == value && inventory.GetSlotCount(i) < inventory.GetSlotCapacity(i, value))
			{
				return i;
			}
		}
		for (int j = num; j < num2; j++)
		{
			if (inventory.GetSlotCount(j) == 0 && inventory.GetSlotCapacity(j, value) > 0)
			{
				return j;
			}
		}
		return -1;
	}

	public static int AcquireItems(IInventory inventory, int value, int count, Range slotRange)
	{
		while (count > 0)
		{
			int num = FindAcquireSlotForItem(inventory, value, slotRange);
			if (num < 0)
			{
				break;
			}
			inventory.AddSlotItems(num, value, 1);
			count--;
		}
		return count;
	}

	public static int FindRemoveSlotForItem(IInventory inventory, int value, int count)
	{
		for (int i = 0; i < inventory.SlotsCount; i++)
		{
			if (inventory.GetSlotCount(i) > 0 && inventory.GetSlotValue(i) == value && inventory.GetSlotCount(i) >= count)
			{
				inventory.RemoveSlotItems(i, count);
				return i;
			}
		}
		return -1;
	}

	public static int FindRemoveTankFluid(ITank tank, int value, float volume)
	{
		for (int i = 0; i < tank.TanksCount; i++)
		{
			if (tank.GetTankVolume(i) > 0f && tank.GetTankValue(i) == value && tank.GetTankVolume(i) >= volume)
			{
				tank.RemoveTankFluid(i, volume);
				return i;
			}
		}
		return -1;
	}
}
