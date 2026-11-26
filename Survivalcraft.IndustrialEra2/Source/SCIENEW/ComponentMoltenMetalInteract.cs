using System;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Utils;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentMoltenMetalInteract : Component, IUpdateable
{
	public ComponentCreature m_componentCreature;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemTime m_subsystemTime;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentCreature = Entity.FindComponent<ComponentCreature>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
	}

	public void Update(float dt)
	{
		Vector3 position = m_componentCreature.ComponentBody.Position;
		int x = Terrain.ToCell(position.X);
		int y = Terrain.ToCell(position.Y + 0.01f);
		int z = Terrain.ToCell(position.Z);
		if (m_subsystemTerrain.Terrain.GetCellContents(x, y, z) == MoltenIronBlock.Index || m_subsystemTerrain.Terrain.GetCellContents(x, y, z) == MoltenSteelBlock.Index)
		{
			m_componentCreature.ComponentHealth.Injure(2f * dt, null, ignoreInvulnerability: false, LanguageUtils.GetText(this, "DieCause"));
			float num = 1.1f + 0.1f * (float)Math.Sin(12.0 * m_subsystemTime.GameTime);
			m_componentCreature.ComponentHealth.m_redScreenFactor = MathUtils.Max(m_componentCreature.ComponentHealth.m_redScreenFactor, num * 1.5f);
			m_componentCreature.ComponentHealth.m_componentOnFire.SetOnFire(null, 8f);
			if (m_subsystemTime.PeriodicGameTimeEvent(3.0, 0.0))
			{
				Project.FindSubsystem<SubsystemAudio>().PlaySound("Audio/SizzleLong", 1f, 0f, position, 4f, autoDelay: true);
			}
		}
	}
}
