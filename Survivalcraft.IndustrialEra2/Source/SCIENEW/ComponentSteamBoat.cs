using System;
using Engine;
using Game;
using GameEntitySystem;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentSteamBoat : ComponentBoat, IUpdateable
{
	private ComponentSteamEngineV componentEngine;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		componentEngine = Entity.FindComponent<ComponentSteamEngineV>();
	}

	public new void Update(float dt)
	{
		bool flag = m_componentBody.ImmersionFactor > 0f;
		if (m_componentDamage.Hitpoints < 0.33f)
		{
			m_componentBody.Density = 1.15f;
			if ((m_componentDamage.Hitpoints - m_componentDamage.HitpointsChange >= 0.33f) & flag)
			{
				m_subsystemAudio.PlaySound("Audio/Sinking", 1f, 0f, m_componentBody.Position, 4f, autoDelay: true);
			}
		}
		else if (m_componentDamage.Hitpoints < 0.66f)
		{
			m_componentBody.Density = 0.7f;
			if ((m_componentDamage.Hitpoints - m_componentDamage.HitpointsChange >= 0.66f) & flag)
			{
				m_subsystemAudio.PlaySound("Audio/Sinking", 1f, 0f, m_componentBody.Position, 4f, autoDelay: true);
			}
		}
		flag = m_componentBody.ImmersionFactor > 0.95f;
		bool flag2 = !flag && m_componentBody.ImmersionFactor > 0.01f && !m_componentBody.StandingOnValue.HasValue && m_componentBody.StandingOnBody == null;
		m_turnSpeed += 2.5f * m_subsystemTime.GameTimeDelta * (1f * TurnOrder - m_turnSpeed);
		Quaternion rotation = m_componentBody.Rotation;
		float num = MathF.Atan2(2f * rotation.Y * rotation.W - 2f * rotation.X * rotation.Z, 1f - 2f * rotation.Y * rotation.Y - 2f * rotation.Z * rotation.Z);
		float num2 = 15f;
		if (componentEngine == null)
		{
			return;
		}
		if (flag2 && componentEngine.IsPowering())
		{
			num -= m_turnSpeed * dt;
		}
		m_componentBody.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, num);
		if (flag2 && MoveOrder != 0f && componentEngine.IsPowering())
		{
			m_componentBody.Velocity += dt * num2 * MoveOrder * m_componentBody.Matrix.Forward;
		}
		if (flag)
		{
			m_componentDamage.Damage(0.005f * dt);
			if (m_componentMount.Rider != null)
			{
				m_componentMount.Rider.StartDismounting();
			}
		}
		MoveOrder = 0f;
		TurnOrder = 0f;
	}
}
