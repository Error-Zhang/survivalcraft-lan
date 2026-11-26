using Engine;
using Game;
using GameEntitySystem;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentInSludgeEffect : Component, IDrawable, IUpdateable
{
	public SubsystemTerrain m_subsystemTerrain;

	public int m_sludgeBlockID;

	public ComponentPlayer m_componentPlayer;

	public ComponentIEScreenOverlays m_componentOverlays;

	public ComponentLocomotion m_componentLocomotion;

	public ComponentBody m_componentBody;

	public int[] DrawOrders => new int[1] { 1100 };

	public UpdateOrder UpdateOrder => UpdateOrder.Locomotion;

	public void Draw(Camera camera, int drawOrder)
	{
		Vector3 viewPosition = camera.ViewPosition;
		int x = Terrain.ToCell(viewPosition.X);
		int y = Terrain.ToCell(viewPosition.Y);
		int z = Terrain.ToCell(viewPosition.Z);
		if (Terrain.ReplaceLight(m_subsystemTerrain.Terrain.GetCellValue(x, y, z), 0) == m_sludgeBlockID && m_componentPlayer != null)
		{
			m_componentOverlays.SludgeFactor = 0.9f;
		}
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_sludgeBlockID = NormalBlockManager.GetNormalBlockID("OilSludge");
		m_componentPlayer = Entity.FindComponent<ComponentPlayer>(throwOnError: false);
		if (m_componentPlayer != null)
		{
			m_componentOverlays = Entity.FindComponent<ComponentIEScreenOverlays>(throwOnError: true);
		}
		m_componentLocomotion = Entity.FindComponent<ComponentLocomotion>(throwOnError: true);
		m_componentBody = Entity.FindComponent<ComponentBody>(throwOnError: true);
	}

	public void Update(float dt)
	{
		Vector3 position = m_componentBody.Position;
		int x = Terrain.ToCell(position.X);
		int y = Terrain.ToCell(position.Y);
		int z = Terrain.ToCell(position.Z);
		if (Terrain.ReplaceLight(m_subsystemTerrain.Terrain.GetCellValue(x, y, z), 0) == m_sludgeBlockID)
		{
			m_componentBody.ApplyImpulse(-m_componentBody.Velocity * 0.5f);
		}
	}
}
