using Game;
using GameEntitySystem;

namespace SCIENEW;

public class ImpactExplosivesItemBehavior : ItemBehavior
{
	public SubsystemExplosions m_subsystemExplosions;

	public ImpactExplosivesItemBehavior(Project project)
		: base(project)
	{
		m_subsystemExplosions = Project.FindSubsystem<SubsystemExplosions>(throwOnError: true);
	}

	public override bool OnHitAsProjectile(CellFace? cellFace, ComponentBody componentBody, WorldItem worldItem)
	{
		return m_subsystemExplosions.TryExplodeBlock(Terrain.ToCell(worldItem.Position.X), Terrain.ToCell(worldItem.Position.Y), Terrain.ToCell(worldItem.Position.Z), worldItem.Value);
	}
}
