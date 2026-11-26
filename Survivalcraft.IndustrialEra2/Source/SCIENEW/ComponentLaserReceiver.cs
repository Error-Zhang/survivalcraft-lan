using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions.GameLogic;
using SCIENEW.VoltNet;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentLaserReceiver : Component, IUpdateable, ILaserHandler
{
	public SubsystemLaser m_subsystemLaser;

	public ComponentBlockEntity m_componentBlockEntity;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemVoltNet m_subsystemVoltNet;

	public int? m_direction;

	public double m_inputEnergy;

	public bool m_clearEnergyRequested;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_subsystemLaser = Project.FindSubsystem<SubsystemLaser>(throwOnError: true);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemVoltNet = Project.FindSubsystem<SubsystemVoltNet>(throwOnError: true);
	}

	public bool OnLaserHit(Vector3 renderStart, Vector3 incident, TerrainRaycastResult hit, Color color, double energy)
	{
		m_inputEnergy = 0.0;
		m_clearEnergyRequested = false;
		if (m_direction.HasValue && hit.CellFace.Face == m_direction.Value)
		{
			m_inputEnergy = energy;
		}
		return false;
	}

	public void Update(float dt)
	{
		if (m_clearEnergyRequested)
		{
			m_inputEnergy = 0.0;
			m_clearEnergyRequested = false;
		}
		if (m_inputEnergy != 0.0)
		{
			m_clearEnergyRequested = true;
		}
		GetDirection();
	}

	public void GetDirection()
	{
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		if (!m_direction.HasValue && m_subsystemTerrain.Terrain.GetChunkAtCell(coordinates.X, coordinates.Z).IsValid())
		{
			int cellValueFast = m_subsystemTerrain.Terrain.GetCellValueFast(m_componentBlockEntity.Coordinates.X, m_componentBlockEntity.Coordinates.Y, m_componentBlockEntity.Coordinates.Z);
			m_direction = SixDirectionModelDevice.GetDirection_Static(cellValueFast);
		}
	}

	public double GetEnergy()
	{
		return m_inputEnergy;
	}
}
