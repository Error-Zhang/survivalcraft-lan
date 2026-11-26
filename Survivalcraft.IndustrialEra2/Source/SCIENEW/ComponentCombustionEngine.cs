using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions.GameLogic;
using SCIENEW.ProductionIO;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentCombustionEngine : ComponentEngine, IUpdateable, IEngineComponent
{
	protected float m_fireTimeRemaining;

	protected float m_heatLevel;

	public float m_speed = 1f;

	public float waterConsumingSpeed = 1f / 75f;

	public SubsystemTerrain m_subsystemTerrain;

	public bool start;

	public PrimitivesRenderer3D m_primitivesRenderer3D = new PrimitivesRenderer3D();

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public float HeatLevel => m_heatLevel;

	public float FireTimeRemaining => m_fireTimeRemaining;

	public override IEnumerable<int> GetSlotIndices(ProductionSlotKind kind)
	{
		return ComponentEngine.m_emptySlotIndices;
	}

	protected override IEnumerable<int> GetTankProductionSlotIndices(ProductionSlotKind kind)
	{
		return kind switch
		{
			ProductionSlotKind.Input => (IEnumerable<int>)ComponentEngine.m_inputTankIndices,
			ProductionSlotKind.Consume => ComponentEngine.m_inputTankIndices,
			_ => ComponentEngine.m_emptyTankIndices,
		};
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
			if (HasGasoline() && start)
			{
				m_fireTimeRemaining = 1f;
				m_heatLevel = 1f;
			}
		}
		else if (HasGasoline() && start)
		{
			m_fireTimeRemaining = 1f;
			m_tanks[0].Volume = MathUtils.Max(0f, m_tanks[0].Volume - dt * waterConsumingSpeed);
			if (m_subsystemTime.PeriodicGameTimeEvent(0.949999988079071, (double)GetHashCode() % 1000.0 / 1000.0))
			{
				UpdateVolume();
				m_subsystemAudio.PlaySound("Audio/CombustionEngine", m_audioVolume, 0f, (m_type == EngineType.Block) ? new Vector3(m_componentBlockEntity.Coordinates) : m_componentBody.Position, 5f, 0f);
			}
		}
		else
		{
			m_fireTimeRemaining = 0f;
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
			int deviceValueByType = DevicesBlockManager.GetDeviceValueByType<CombustionEngineDevice>((m_heatLevel > 0f) ? (4 | direction_Static) : direction_Static);
			m_subsystemTerrain.ChangeCell(coordinates.X, coordinates.Y, coordinates.Z, deviceValueByType);
		}
	}

	private bool HasGasoline()
	{
		if (GetTankValue(0) == FluidsManager.GetFluidID("C:Gasoline", 0))
		{
			return GetTankVolume(0) > 0f;
		}
		return false;
	}

	public new bool IsPowering()
	{
		return m_heatLevel > 0f;
	}
}
