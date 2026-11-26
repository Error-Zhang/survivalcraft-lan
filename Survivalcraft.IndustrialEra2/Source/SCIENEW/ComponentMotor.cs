using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions.GameLogic;
using SCIENEW.VoltNet;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentMotor : ComponentEngine, IUpdateable, IRotatableDeviceComponent
{
	public SubsystemTerrain m_subsystemTerrain;

	public CellFace m_powerOutputFace;

	public bool m_isPowering;

	protected bool m_isAddedToSubsystem;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public override bool IsPowering()
	{
		return m_isPowering;
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
	}

	public override void OnEntityAdded()
	{
	}

	public override void AddEngine()
	{
		int direction_Static = SixDirectionModelDevice.GetDirection_Static(m_subsystemTerrain.Terrain.GetCellValueFast(m_componentBlockEntity.Coordinates.X, m_componentBlockEntity.Coordinates.Y, m_componentBlockEntity.Coordinates.Z));
		m_powerOutputFace = new CellFace(m_componentBlockEntity.Coordinates.X, m_componentBlockEntity.Coordinates.Y, m_componentBlockEntity.Coordinates.Z, direction_Static);
		m_subsystemEnginePower.Add(m_powerOutputFace, this);
	}

	public override void OnEntityRemoved()
	{
		if (m_type == EngineType.Block)
		{
			RemoveEngine();
		}
	}

	public override void RemoveEngine()
	{
		m_subsystemEnginePower.Remove(m_powerOutputFace);
	}

	public void Update(float dt)
	{
		if (m_type == EngineType.Block)
		{
			Point3 coordinates = m_componentBlockEntity.Coordinates;
			if (m_subsystemTerrain.Terrain.GetChunkAtCell(coordinates.X, coordinates.Z).IsValid() && !m_isAddedToSubsystem && m_type == EngineType.Block)
			{
				AddEngine();
				m_isAddedToSubsystem = true;
			}
			if (Project.FindSubsystem<SubsystemVoltNet>(throwOnError: true).GetVoltElement(coordinates) is MotorVoltElement motorVoltElement)
			{
				m_isPowering = motorVoltElement.CurrentWorkState == WorkState.Active;
			}
			else
			{
				m_isPowering = false;
			}
		}
	}

	public void OnRotated(int oldValue, int newValue)
	{
		RemoveEngine();
		AddEngine();
	}
}
