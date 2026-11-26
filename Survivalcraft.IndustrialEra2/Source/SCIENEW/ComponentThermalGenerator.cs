using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions.GameLogic;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentThermalGenerator : ComponentInventoryTankBase, IUpdateable
{
	public float m_speed = 1f;

	public float m_consumingSpeed = 0.02f;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemExplosions m_subsystemExplosions;

	public ComponentBlockEntity m_componentBlockEntity;

	private float m_consumedMagma;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public bool IsWorking { get; private set; }

	public override int GetSlotCapacity(int slotIndex, int value)
	{
		if (value != BasaltBlock.Index)
		{
			return 0;
		}
		return base.GetSlotCapacity(slotIndex, value);
	}

	public override float GetTankCapacity(int tankIndex, int value)
	{
		if (value != FluidsManager.GetFluidID("MagmaFluid", 0))
		{
			return 0f;
		}
		return base.GetTankCapacity(tankIndex, value);
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_tanks[0].Capacity = 24f;
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemExplosions = Project.FindSubsystem<SubsystemExplosions>(throwOnError: true);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_consumedMagma = valuesDictionary.GetValue("ConsumedMagma", 0f);
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
		valuesDictionary.SetValue("ConsumedMagma", m_consumedMagma);
	}

	public void Update(float dt)
	{
		if (HasMagma() && ComponentInventoryBase.FindAcquireSlotForItem(this, BasaltBlock.Index) >= 0)
		{
			IsWorking = true;
			m_consumedMagma += RemoveTankFluid(0, dt * m_consumingSpeed);
			if (m_consumedMagma > 4f)
			{
				ComponentInventoryBase.AcquireItems(this, BasaltBlock.Index, 1);
				m_consumedMagma = 0f;
			}
		}
		else
		{
			IsWorking = false;
		}
		ReplaceFurnace(m_componentBlockEntity.Coordinates);
	}

	public virtual void ReplaceFurnace(Point3 coordinates)
	{
		if (m_subsystemTerrain.Terrain.GetChunkAtCell(coordinates.X, coordinates.Z).IsValid())
		{
			int direction_Static = IlluminableDevice.GetDirection_Static(m_subsystemTerrain.Terrain.GetCellValue(coordinates.X, coordinates.Y, coordinates.Z));
			int deviceValueByType = DevicesBlockManager.GetDeviceValueByType<ThermalGeneratorDevice>(IsWorking ? (4 | direction_Static) : direction_Static);
			m_subsystemTerrain.ChangeCell(coordinates.X, coordinates.Y, coordinates.Z, deviceValueByType);
		}
	}

	private bool HasMagma()
	{
		if (GetTankValue(0) == FluidsManager.GetFluidID("MagmaFluid", 0))
		{
			return GetTankVolume(0) > 0f;
		}
		return false;
	}
}
