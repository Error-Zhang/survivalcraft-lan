using Game;
using GameEntitySystem;
using SCIENEW.Utils;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentRadiationDamage : Component, IUpdateable
{
	public SubsystemTime m_subsystemTime;

	public ComponentHealth m_componentHealth;

	public SubsystemRadiation m_subsystemRadiations;

	public ComponentBody m_componentBody;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
		m_componentHealth = Entity.FindComponent<ComponentHealth>(throwOnError: true);
		m_componentBody = Entity.FindComponent<ComponentBody>(throwOnError: true);
		m_subsystemRadiations = Project.FindSubsystem<SubsystemRadiation>(throwOnError: true);
	}

	public void Update(float dt)
	{
		if (m_subsystemTime.PeriodicGameTimeEvent(5.0, 0.0))
		{
			float radiation = m_subsystemRadiations.GetRadiation(m_componentBody.Position);
			if (radiation >= 10f)
			{
				m_componentHealth.Injure(0.01f * radiation, null, ignoreInvulnerability: false, LanguageUtils.GetText(this, "DeathCause"));
			}
		}
	}
}
