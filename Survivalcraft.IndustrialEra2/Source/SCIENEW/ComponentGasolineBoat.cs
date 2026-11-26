using System;
using Engine;
using Game;
using GameEntitySystem;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentGasolineBoat : ComponentBoat, IUpdateable
{
	private ComponentCombustionEngine m_componentEngine;

	public ComponentPropellerModel m_componentPropellerModel;

	private float m_propellerRotateSpeed;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentEngine = Entity.FindComponent<ComponentCombustionEngine>(throwOnError: true);
		m_componentPropellerModel = Entity.FindComponent<ComponentPropellerModel>(throwOnError: true);
	}

	public new void Update(float deltaTime)
	{
		m_componentMount.MountOffset = (IESettingsManager.Get<bool>("GasolineBoatMountOffset") ? new Vector3(0f, 0.85f, 0.6f) : new Vector3(0f, 0.85f, -1.2f));
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
		bool flag2 = m_componentBody.ImmersionFactor > 0.95f;
		bool flag3 = !flag2 && m_componentBody.ImmersionFactor > 0.01f && !m_componentBody.StandingOnValue.HasValue && m_componentBody.StandingOnBody == null;
		m_turnSpeed += 2.5f * m_subsystemTime.GameTimeDelta * (1f * TurnOrder - m_turnSpeed);
		Quaternion rotation = m_componentBody.Rotation;
		float num = MathF.Atan2(2f * rotation.Y * rotation.W - 2f * rotation.X * rotation.Z, 1f - 2f * rotation.Y * rotation.Y - 2f * rotation.Z * rotation.Z);
		float num2 = 30f;
		if (m_componentEngine == null)
		{
			return;
		}
		if (flag3 && m_componentEngine.IsPowering())
		{
			num -= m_turnSpeed * deltaTime;
		}
		m_componentBody.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, num);
		if (flag3 && MoveOrder != 0f && m_componentEngine.IsPowering())
		{
			m_componentBody.Velocity += deltaTime * num2 * MoveOrder * m_componentBody.Matrix.Forward;
		}
		if (flag2)
		{
			m_componentDamage.Damage(0.005f * deltaTime);
			if (m_componentMount.Rider != null)
			{
				m_componentMount.Rider.StartDismounting();
			}
		}
		float x = (m_componentEngine.IsPowering() ? (50f * MoveOrder) : 0f);
		m_propellerRotateSpeed = MathUtils.Lerp(m_propellerRotateSpeed, x, deltaTime * 10f);
		m_componentPropellerModel.Rotate(deltaTime, m_propellerRotateSpeed);
		MoveOrder = 0f;
		TurnOrder = 0f;
	}
}
