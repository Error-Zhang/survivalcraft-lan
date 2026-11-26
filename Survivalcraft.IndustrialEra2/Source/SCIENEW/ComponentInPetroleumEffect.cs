using Engine;
using Game;
using GameEntitySystem;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentInPetroleumEffect : Component, IDrawable, IUpdateable
{
	public SubsystemTerrain m_subsystemTerrain;

	public ComponentPlayer m_componentPlayer;

	public ComponentIEScreenOverlays m_componentOverlays;

	public ComponentLocomotion m_componentLocomotion;

	public ComponentBody m_componentBody;

	public SubsystemFluidBlockBehavior m_subsystemFluidBlockBehavior;

	public int[] DrawOrders => new int[1] { 1100 };

	public UpdateOrder UpdateOrder => UpdateOrder.Locomotion;

	public void Draw(Camera camera, int drawOrder)
	{
		Vector3 viewPosition = camera.ViewPosition;
		int x = Terrain.ToCell(viewPosition.X);
		int y = Terrain.ToCell(viewPosition.Y);
		int z = Terrain.ToCell(viewPosition.Z);
		float? surfaceHeight = m_subsystemFluidBlockBehavior.GetSurfaceHeight(x, y, z, out var surfaceFluidBlock);
		if (surfaceHeight.HasValue && surfaceFluidBlock is PetroleumBlock && m_componentOverlays != null)
		{
			float num = surfaceHeight.Value + 0.1f - viewPosition.Y;
			m_componentOverlays.PetroleumFactor = ((num > 0f) ? 0.9f : 0f);
		}
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_componentPlayer = Entity.FindComponent<ComponentPlayer>(throwOnError: false);
		if (m_componentPlayer != null)
		{
			m_componentOverlays = Entity.FindComponent<ComponentIEScreenOverlays>(throwOnError: true);
		}
		m_componentLocomotion = Entity.FindComponent<ComponentLocomotion>(throwOnError: true);
		m_componentBody = Entity.FindComponent<ComponentBody>(throwOnError: true);
		m_subsystemFluidBlockBehavior = Project.FindSubsystem<SubsystemFluidBlockBehavior>(throwOnError: true);
	}

	public void Update(float dt)
	{
		Vector3 position = m_componentBody.Position;
		int x = Terrain.ToCell(position.X);
		int y = Terrain.ToCell(position.Y);
		int z = Terrain.ToCell(position.Z);
		float? surfaceHeight = m_subsystemFluidBlockBehavior.GetSurfaceHeight(x, y, z, out var surfaceFluidBlock);
		if (surfaceHeight.HasValue && surfaceFluidBlock is PetroleumBlock && surfaceHeight.Value + 0.1f - position.Y > 0f)
		{
			m_componentBody.ApplyImpulse(-m_componentBody.Velocity * 0.3f);
		}
	}
}
