using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions.GameLogic;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentNuclearEngine : ComponentEngine, IUpdateable
{
	public SubsystemRadiation m_subsystemRadiation;

	public SubsystemTerrain m_subsystemTerrain;

	public float m_fireTimeRemaining;

	public float m_radiationLevel;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public bool Ignited { get; set; }

	public override int GetSlotCapacity(int slotIndex, int value)
	{
		if (!IsFuelRod(value))
		{
			return 0;
		}
		return base.GetSlotCapacity(slotIndex, value);
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_subsystemRadiation = Project.FindSubsystem<SubsystemRadiation>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_fireTimeRemaining = valuesDictionary.GetValue("FireTimeRemaining", 0f);
		m_radiationLevel = valuesDictionary.GetValue("RadiationLevel", 0f);
		Ignited = valuesDictionary.GetValue("Ignited", defaultValue: false);
	}

	public bool IsFuelRod(int value)
	{
		if (Terrain.ExtractContents(value) == BaseDamageableItemBlock.Index)
		{
			if (DamageableItemManager.GetItem(value) is FuelRodDItem fuelRodDItem)
			{
				return fuelRodDItem.m_type == FuelRodDItem.RodType.UFuelRod;
			}
			return false;
		}
		return false;
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
		valuesDictionary.SetValue("FireTimeRemaining", m_fireTimeRemaining);
		valuesDictionary.SetValue("RadiationLevel", m_radiationLevel);
		valuesDictionary.SetValue("Ignited", Ignited);
	}

	public override bool IsPowering()
	{
		if (m_radiationLevel > 0f)
		{
			return Ignited;
		}
		return false;
	}

	public void Update(float dt)
	{
		int slotValue = GetSlotValue(0);
		if (IsFuelRod(slotValue) && Ignited)
		{
			m_radiationLevel += dt * 3f;
			m_fireTimeRemaining = 1000f;
			if (m_subsystemTime.PeriodicGameTimeEvent(24.0, GetHashCode() % 1000 / 1000))
			{
				RemoveSlotItems(0, 1);
				AddSlotItems(0, DamageableItemManager.DamageItem(slotValue, 1), 1);
			}
		}
		else
		{
			m_fireTimeRemaining = 0f;
			m_radiationLevel = 0f;
		}
		if (m_type == EngineType.Block)
		{
			ReplaceFurnace(m_componentBlockEntity.Coordinates);
			if (IsPowering())
			{
				m_subsystemRadiation.Add(m_componentBlockEntity.Coordinates, m_radiationLevel);
			}
		}
		else if (m_type == EngineType.Vehicle && IsPowering())
		{
			m_subsystemRadiation.Add(Terrain.ToCell(m_componentBody.Position), m_radiationLevel);
		}
	}

	public virtual void ReplaceFurnace(Point3 coordinates)
	{
		if (m_subsystemTerrain.Terrain.GetChunkAtCell(coordinates.X, coordinates.Z).IsValid())
		{
			int direction_Static = IlluminableDevice.GetDirection_Static(m_subsystemTerrain.Terrain.GetCellValue(coordinates.X, coordinates.Y, coordinates.Z));
			int deviceValueByType = DevicesBlockManager.GetDeviceValueByType<NuclearEngineDevice>(IsPowering() ? (4 | direction_Static) : direction_Static);
			m_subsystemTerrain.ChangeCell(coordinates.X, coordinates.Y, coordinates.Z, deviceValueByType);
		}
	}
}
