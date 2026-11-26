using Game;
using GameEntitySystem;
using SCIENEW.Screens;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentSubsystemScreensOperator : Component, IUpdateable
{
	public ComponentMiner m_componentMiner;

	public SubsystemScreens m_subsystemScreens;

	public int m_linkerId;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentMiner = Entity.FindComponent<ComponentMiner>(throwOnError: true);
		m_subsystemScreens = Project.FindSubsystem<SubsystemScreens>(throwOnError: true);
		m_linkerId = ItemsBlockManager.GetItemID("MonitorLinker");
		if (valuesDictionary.GetValue("LinksCount", 0) > 0)
		{
			m_subsystemScreens.ImportLegacyLinks(valuesDictionary, Entity);
		}
	}

	public void Update(float dt)
	{
		m_subsystemScreens.m_debugCanDraw = m_componentMiner.ActiveBlockValue == m_linkerId;
	}
}
