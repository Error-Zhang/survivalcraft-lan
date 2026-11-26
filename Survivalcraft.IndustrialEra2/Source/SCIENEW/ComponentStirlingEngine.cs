using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions.GameLogic;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentStirlingEngine : ComponentEngine, IUpdateable, IEngineComponent
{
	protected float m_fireTimeRemaining;

	protected float m_heatLevel;

	protected float m_fireTime;

	public float m_speed = 1f;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemExplosions m_subsystemExplosions;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public float HeatLevel => m_heatLevel;

	public float FireTimeRemaining => m_fireTimeRemaining;

	public float FireTime => m_fireTime;

	public override int GetSlotCapacity(int slotIndex, int value)
	{
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
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
		valuesDictionary.SetValue("FireTimeRemaining", m_fireTimeRemaining);
		valuesDictionary.SetValue("HeatLevel", m_heatLevel);
		valuesDictionary.SetValue("FireTime", m_fireTime);
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
					if (m_type == EngineType.Block)
					{
						m_subsystemExplosions.TryExplodeBlock(m_componentBlockEntity.Coordinates.X, m_componentBlockEntity.Coordinates.Y, m_componentBlockEntity.Coordinates.Z, slotValue);
					}
					else
					{
						m_subsystemExplosions.AddExplosion((int)m_componentBody.Position.X, (int)m_componentBody.Position.Y, (int)m_componentBody.Position.Z, block.GetExplosionPressure(slotValue), isIncendiary: false, noExplosionSound: false);
					}
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
			m_fireTimeRemaining = MathUtils.Max(0f, m_fireTimeRemaining - dt * 2f);
			if (m_subsystemTime.PeriodicGameTimeEvent(1.9500000476837158, (double)GetHashCode() % 1000.0 / 1000.0))
			{
				UpdateVolume();
				m_subsystemAudio.PlaySound("Audio/StirlingEngine", m_audioVolume, 0f, (m_type == EngineType.Block) ? new Vector3(m_componentBlockEntity.Coordinates) : m_componentBody.Position, 5f, 0f);
			}
		}
		if (m_type == EngineType.Block)
		{
			ReplaceFurnace(m_componentBlockEntity.Coordinates);
		}
	}

	public virtual void ReplaceFurnace(Point3 coordinates)
	{
		if (m_subsystemTerrain.Terrain.GetChunkAtCell(coordinates.X, coordinates.Z).IsValid())
		{
			int direction_Static = IlluminableDevice.GetDirection_Static(m_subsystemTerrain.Terrain.GetCellValue(coordinates.X, coordinates.Y, coordinates.Z));
			int deviceValueByType = DevicesBlockManager.GetDeviceValueByType<StirlingEngineDevice>((m_heatLevel > 0f) ? (4 | direction_Static) : direction_Static);
			m_subsystemTerrain.ChangeCell(coordinates.X, coordinates.Y, coordinates.Z, deviceValueByType);
		}
	}

	public new bool IsPowering()
	{
		return m_heatLevel > 0f;
	}
}
