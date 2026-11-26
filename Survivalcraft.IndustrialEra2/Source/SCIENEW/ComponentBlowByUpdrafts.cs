using Game;
using GameEntitySystem;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentBlowByUpdrafts : Component, IUpdateable
{
	public ComponentBody m_componentBody;

	public SubsystemUpdrafts m_subsystemUpdrafts;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentBody = Entity.FindComponent<ComponentBody>(throwOnError: true);
		m_subsystemUpdrafts = Project.FindSubsystem<SubsystemUpdrafts>(throwOnError: true);
	}

	public void Update(float dt)
	{
		m_componentBody.ApplyImpulse(m_subsystemUpdrafts.GetForce(m_componentBody.Position));
	}
}
