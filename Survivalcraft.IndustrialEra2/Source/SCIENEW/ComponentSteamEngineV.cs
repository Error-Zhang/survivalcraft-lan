using Engine;
using Game;
using GameEntitySystem;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentSteamEngineV : ComponentInventoryTankBase, IUpdateable, IEngineComponent
{
	protected float m_fireTimeRemaining;

	protected float m_heatLevel;

	public float m_speed = 1f;

	public float waterConsumingSpeed = 4f / 75f;

	public bool start;

	public SubsystemTerrain m_subsystemTerrain;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public float HeatLevel => m_heatLevel;

	public float FireTimeRemaining => m_fireTimeRemaining;

	public int FuelSlotIndex { get; set; }

	public override int GetSlotCapacity(int slotIndex, int value)
	{
		if (slotIndex == FuelSlotIndex)
		{
			if (!(BlocksManager.Blocks[Terrain.ExtractContents(value)].GetFuelHeatLevel(value) > 0f))
			{
				return 0;
			}
			return base.GetSlotCapacity(slotIndex, value);
		}
		return base.GetSlotCapacity(slotIndex, value);
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_tanks[0].Capacity = 16f;
		m_fireTimeRemaining = valuesDictionary.GetValue("FireTimeRemaining", 0f);
		m_heatLevel = valuesDictionary.GetValue("HeatLevel", 0f);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		start = valuesDictionary.GetValue("Start", defaultValue: true);
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
		valuesDictionary.SetValue("FireTimeRemaining", m_fireTimeRemaining);
		valuesDictionary.SetValue("HeatLevel", m_heatLevel);
		valuesDictionary.SetValue("Start", start);
	}

	public void Update(float dt)
	{
		if (m_fireTimeRemaining <= 0f)
		{
			if (m_heatLevel > 0f)
			{
				m_heatLevel = 0f;
			}
			if (GetSlotCount(0) > 0 && start)
			{
				int slotValue = GetSlotValue(0);
				Block block = BlocksManager.Blocks[Terrain.ExtractContents(slotValue)];
				if (block.GetFuelFireDuration(slotValue) > 0f && HasWater())
				{
					m_fireTimeRemaining = block.GetFuelFireDuration(slotValue);
					m_heatLevel = block.GetFuelHeatLevel(slotValue);
					m_slots[0].Count--;
				}
			}
		}
		else if (HasWater() && start)
		{
			m_fireTimeRemaining = MathUtils.Max(0f, m_fireTimeRemaining - dt * m_speed);
			m_tanks[0].Volume = MathUtils.Max(0f, m_tanks[0].Volume - dt * waterConsumingSpeed);
		}
		else
		{
			m_fireTimeRemaining = 0f;
		}
	}

	private bool HasWater()
	{
		if (GetTankValue(0) == FluidsManager.GetFluidID("WaterFluid", 0))
		{
			return GetTankVolume(0) > 0f;
		}
		return false;
	}

	public bool IsPowering()
	{
		return m_heatLevel > 0f;
	}
}
