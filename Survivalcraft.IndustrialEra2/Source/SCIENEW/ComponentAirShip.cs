using System;
using Engine;
using Game;
using GameEntitySystem;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentAirShip : ComponentBoat, IUpdateable
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
		Quaternion rotation = componentBody.Rotation;
		componentBody.IsGravityEnabled = false;
		componentBody.IsGroundDragEnabled = false;
		float num = MathF.Atan2(2f * rotation.Y * rotation.W - 2f * rotation.X * rotation.Z, 1f - 2f * rotation.Y * rotation.Y - 2f * rotation.Z * rotation.Z);
		if ((m_turnSpeed += 2.5f * dt * (TurnOrder - m_turnSpeed)) != 0f && componentEngine.HeatLevel > 0f)
		{
			num -= m_turnSpeed * dt;
		}
		componentBody.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, num);
		ComponentRider rider = m_componentMount.Rider;
		if (MoveOrder != 0f && rider != null)
		{
			componentBody.Velocity += dt * ((componentEngine != null) ? (componentEngine.HeatLevel * 20f) : 0f) * MoveOrder * rider.ComponentCreature.ComponentCreatureModel.EyeRotation.GetForwardVector();
		}
		if (componentBody.m_position.Y >= 220f)
		{
			componentBody.m_velocity = new Vector3(componentBody.m_velocity.X, MathUtils.Min(componentBody.m_velocity.Y, 0f), componentBody.m_velocity.Z);
		}
		if (componentBody.ImmersionFactor > 0.95f)
		{
			m_componentDamage.Damage(0.005f * dt);
			rider?.StartDismounting();
		}
		float x = (componentEngine.IsPowering() ? (50f * MoveOrder) : 0f);
		m_propellerRotateSpeed = MathUtils.Lerp(m_propellerRotateSpeed, x, dt * 10f);
		m_componentPropellerModel.Rotate(dt, m_propellerRotateSpeed);
		TurnOrder = 0f;
		MoveOrder = 0f;
	}
}
