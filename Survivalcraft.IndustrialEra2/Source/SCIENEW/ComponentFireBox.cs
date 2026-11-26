using System;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions.GameLogic;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentFireBox : ComponentInventoryBase, IUpdateable, IFireBoxComponent
{
	protected float m_fireTimeRemaining;

	protected float m_heatLevel;

	protected float m_fireTime;

	public float m_speed = 1f;

	public float m_ashesProbability = 0.1f;

	private int m_ashesValue;

	private ItemBlock m_ashesBlock;

	private new Game.Random m_random = new Game.Random();

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemExplosions m_subsystemExplosions;

	public ComponentBlockEntity m_componentBlockEntity;

	public SubsystemFireBoxes m_subsystemFireBoxes;

	public SubsystemTime m_subsystemTime;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public float HeatLevel => m_heatLevel;

	public float FireTimeRemaining => m_fireTimeRemaining;

	public int AshesSlotIndex => 1;

	public float FireTime => m_fireTime;

	public override int GetSlotCapacity(int slotIndex, int value)
	{
		if (slotIndex == 1)
		{
			return 0;
		}
		if (!(BlocksManager.Blocks[Terrain.ExtractContents(value)].GetFuelHeatLevel(value) > 0f))
		{
			return 0;
		}
		return base.GetSlotCapacity(slotIndex, value);
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_fireTimeRemaining = valuesDictionary.GetValue("FireTimeRemaining", 0f);
		m_heatLevel = valuesDictionary.GetValue("HeatLevel", 0f);
		m_fireTime = valuesDictionary.GetValue("FireTime", 0f);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemExplosions = Project.FindSubsystem<SubsystemExplosions>(throwOnError: true);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemFireBoxes = Project.FindSubsystem<SubsystemFireBoxes>(throwOnError: true);
		m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
		m_ashesValue = ItemsBlockManager.GetItemID("Ashes");
		m_ashesBlock = ItemsBlockManager.GetItem("Ashes");
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
		valuesDictionary.SetValue("FireTimeRemaining", m_fireTimeRemaining);
		valuesDictionary.SetValue("HeatLevel", m_heatLevel);
		valuesDictionary.SetValue("FireTime", m_fireTime);
	}

	public override void OnEntityAdded()
	{
		base.OnEntityAdded();
		m_subsystemFireBoxes.Add(m_componentBlockEntity.Coordinates, this);
	}

	public override void OnEntityRemoved()
	{
		base.OnEntityRemoved();
		m_subsystemFireBoxes.Remove(m_componentBlockEntity.Coordinates);
	}

	public void Update(float dt)
	{
		if (m_fireTimeRemaining <= 0f)
		{
			if (m_heatLevel > 0f)
			{
				m_heatLevel = 0f;
			}
			if (GetSlotCount(0) > 0)
			{
				int slotValue = GetSlotValue(0);
				Block block = BlocksManager.Blocks[Terrain.ExtractContents(slotValue)];
				if (block.GetExplosionPressure(slotValue) > 0f)
				{
					m_slots[0].Count--;
					m_subsystemExplosions.TryExplodeBlock(m_componentBlockEntity.Coordinates.X, m_componentBlockEntity.Coordinates.Y, m_componentBlockEntity.Coordinates.Z, slotValue);
				}
				if (block.GetFuelFireDuration(slotValue) > 0f)
				{
					m_fireTime = block.GetFuelFireDuration(slotValue);
					m_fireTimeRemaining = block.GetFuelFireDuration(slotValue);
					m_heatLevel = block.GetFuelHeatLevel(slotValue);
					m_slots[0].Count--;
				}
			}
		}
		else
		{
			m_fireTimeRemaining = MathUtils.Max(0f, m_fireTimeRemaining - dt);
			if (m_subsystemTime.PeriodicGameTimeEvent(1.0, (double)GetHashCode() % 1000.0 / 1000.0) && m_random.Float() < m_ashesProbability)
			{
				m_random.Seed();
				m_slots[AshesSlotIndex].Value = m_ashesValue;
				m_slots[AshesSlotIndex].Count = Math.Min(m_slots[AshesSlotIndex].Count + 1, m_ashesBlock.GetMaxStacking(m_ashesValue));
			}
		}
		ReplaceFurnace(m_componentBlockEntity.Coordinates);
	}

	public virtual void ReplaceFurnace(Point3 coordinates)
	{
		if (m_subsystemTerrain.Terrain.GetChunkAtCell(coordinates.X, coordinates.Z).IsValid())
		{
			int direction_Static = IlluminableDevice.GetDirection_Static(m_subsystemTerrain.Terrain.GetCellValue(coordinates.X, coordinates.Y, coordinates.Z));
			int deviceValueByType = DevicesBlockManager.GetDeviceValueByType<FireBoxDevice>((m_heatLevel > 0f) ? (4 | direction_Static) : direction_Static);
			m_subsystemTerrain.ChangeCell(coordinates.X, coordinates.Y, coordinates.Z, deviceValueByType);
		}
	}

	public bool IsPreheating()
	{
		return m_heatLevel > 0f;
	}
}
