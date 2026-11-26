using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions.GameLogic;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentLaserEmitter : Component, IUpdateable, ILaserHandler
{
	public SubsystemLaser m_subsystemLaser;

	public ComponentBlockEntity m_componentBlockEntity;

	public SubsystemTerrain m_subsystemTerrain;

	public int? m_direction;

	public double m_energy;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_subsystemLaser = Project.FindSubsystem<SubsystemLaser>(throwOnError: true);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
	}

	public void Update(float dt)
	{
		GetDirection();
		if (m_direction.HasValue)
		{
			Vector3 vector = new Vector3(m_componentBlockEntity.Coordinates) + new Vector3(0.5f);
			m_subsystemLaser.Spawn(vector + CellFace.FaceToVector3(m_direction.Value) * 0.51f, vector + CellFace.FaceToVector3(m_direction.Value) * 0.51f, CellFace.FaceToVector3(m_direction.Value), Color.Red * new Color(255, 255, 255, 128), m_energy);
		}
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

	public bool OnLaserHit(Vector3 renderStart, Vector3 incident, TerrainRaycastResult hit, Color color, double energy)
	{
		return false;
	}
}
