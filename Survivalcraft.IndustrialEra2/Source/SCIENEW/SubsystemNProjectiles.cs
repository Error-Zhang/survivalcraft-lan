using Game;
using GameEntitySystem;
using TemplatesDatabase;

namespace SCIENEW;

public class SubsystemNProjectiles : Subsystem, IUpdateable
{
	public SubsystemUpdrafts m_subsystemUpdrafts;

	public SubsystemProjectiles m_subsystemProjectiles;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public override void Load(ValuesDictionary valuesDictionary)
	{
		base.Load(valuesDictionary);
		m_subsystemUpdrafts = Project.FindSubsystem<SubsystemUpdrafts>(throwOnError: true);
		m_subsystemProjectiles = Project.FindSubsystem<SubsystemProjectiles>(throwOnError: true);
	}

	public void Update(float dt)
	{
		foreach (Projectile projectile in m_subsystemProjectiles.Projectiles)
		{
			if (!projectile.ToRemove)
			{
				projectile.Velocity += m_subsystemUpdrafts.GetForce(projectile.Position) * dt * 60f;
			}
		}
	}
}
