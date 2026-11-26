using System;
using Engine;
using Game;
using GameEntitySystem;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentCar : ComponentBoat, IUpdateable
{
	private ComponentCombustionEngine componentEngine;

	public ComponentPropellerModel m_componentPropellerModel;

	private float m_propellerRotateSpeed;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		componentEngine = Entity.FindComponent<ComponentCombustionEngine>();
		m_componentPropellerModel = Entity.FindComponent<ComponentPropellerModel>();
	}

	public new void Update(float dt)
	{
		ComponentBody componentBody = m_componentBody;
		componentBody.IsGravityEnabled = true;
		componentBody.IsGroundDragEnabled = false;
		Quaternion rotation = componentBody.Rotation;
		float num = MathF.Atan2(2f * rotation.Y * rotation.W - 2f * rotation.X * rotation.Z, 1f - 2f * rotation.Y * rotation.Y - 2f * rotation.Z * rotation.Z);
		if ((m_turnSpeed += 2.5f * dt * (TurnOrder - m_turnSpeed)) != 0f && componentEngine.HeatLevel > 0f)
		{
			num -= m_turnSpeed * dt;
		}
		componentBody.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, num);
		ComponentRider rider = m_componentMount.Rider;
		if (MoveOrder != 0f && rider != null && componentBody.StandingOnValue.HasValue)
		{
			ComponentCombustionEngine componentCombustionEngine = componentEngine;
			if (componentCombustionEngine != null && componentCombustionEngine.HeatLevel > 0f)
			{
				m_componentBody.Velocity += dt * 40f * MoveOrder * m_componentBody.Matrix.Forward;
			}
		}
		if (componentBody.ImmersionFactor > 0.95f)
		{
			m_componentDamage.Damage(0.005f * dt);
			rider?.StartDismounting();
		}
		float x = (componentEngine.IsPowering() ? (50f * MoveOrder) : 0f);
		m_propellerRotateSpeed = MathUtils.Lerp(m_propellerRotateSpeed, x, dt * 10f);
		m_componentPropellerModel.Rotate(dt, m_propellerRotateSpeed);
		MoveOrder = 0f;
		TurnOrder = 0f;
		if (!(componentEngine.HeatLevel <= 0f) && !(componentBody.Mass > 1200f))
		{
			_ = componentBody.StandingOnValue.HasValue;
		}
	}
}
