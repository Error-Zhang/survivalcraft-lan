using System;
using Engine;
using Game;
using GameEntitySystem;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentClimbIEBlocks : Component, IUpdateable
{
	public ComponentLocomotion m_componentLocomotion;

	public int? LadderValue { get; set; }

	public UpdateOrder UpdateOrder => UpdateOrder.Locomotion;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentLocomotion = Entity.FindComponent<ComponentLocomotion>(throwOnError: true);
	}

	public void Update(float dt)
	{
		if (m_componentLocomotion.m_componentCreature.ComponentHealth.Health > 0f && m_componentLocomotion.StunTime <= 0f)
		{
			Vector3 position = m_componentLocomotion.m_componentCreature.ComponentBody.Position;
			int x = Terrain.ToCell(position.X);
			int y = Terrain.ToCell(position.Y + 0.2f);
			int z = Terrain.ToCell(position.Z);
			int cellValue = m_componentLocomotion.m_subsystemTerrain.Terrain.GetCellValue(x, y, z);
			if (m_componentLocomotion.LadderSpeed > 0f && !LadderValue.HasValue && BlockInterfaceResolver.ResolveFromValue<ILadderDevice>(cellValue) != null && m_componentLocomotion.m_subsystemTime.GameTime >= m_componentLocomotion.m_ladderActivationTime && !m_componentLocomotion.IsCreativeFlyEnabled && m_componentLocomotion.m_componentCreature.ComponentBody.ParentBody == null && (m_componentLocomotion.m_componentCreature.ComponentBody.CollisionVelocityChange.Z != 0f || m_componentLocomotion.m_componentCreature.ComponentBody.CollisionVelocityChange.X != 0f || !m_componentLocomotion.m_componentCreature.ComponentBody.StandingOnValue.HasValue))
			{
				LadderValue = cellValue;
				m_componentLocomotion.m_ladderActivationTime = m_componentLocomotion.m_subsystemTime.GameTime + 0.20000000298023224;
				m_componentLocomotion.m_componentCreature.ComponentCreatureSounds.PlayFootstepSound(1f);
			}
			if (LadderValue.HasValue)
			{
				LadderMovement(dt, cellValue);
			}
		}
	}

	public virtual void LadderMovement(float dt, int value)
	{
		m_componentLocomotion.m_componentCreature.ComponentBody.IsGravityEnabled = false;
		Vector3 position = m_componentLocomotion.m_componentCreature.ComponentBody.Position;
		Vector3 velocity = m_componentLocomotion.m_componentCreature.ComponentBody.Velocity;
		if (BlockInterfaceResolver.ResolveFromValue<ILadderDevice>(value) != null)
		{
			LadderValue = value;
			if (m_componentLocomotion.m_componentPlayer.ComponentInput.PlayerInput.Move != Vector3.Zero)
			{
				Vector2 vector = new Vector2(m_componentLocomotion.m_componentPlayer.ComponentInput.PlayerInput.Move.X, m_componentLocomotion.m_componentPlayer.ComponentInput.PlayerInput.Move.Z);
				float num = m_componentLocomotion.LadderSpeed * vector.Y;
				velocity.X = 5f * (MathF.Floor(position.X) + 0.5f - position.X);
				velocity.Z = 5f * (MathF.Floor(position.Z) + 0.5f - position.Z);
				velocity.Y += MathUtils.Saturate(20f * dt) * (num - velocity.Y);
				m_componentLocomotion.m_climbing = true;
			}
			if (m_componentLocomotion.m_componentCreature.ComponentBody.StandingOnValue.HasValue && m_componentLocomotion.m_subsystemTime.GameTime >= m_componentLocomotion.m_ladderActivationTime)
			{
				LadderValue = null;
				m_componentLocomotion.m_ladderActivationTime = m_componentLocomotion.m_subsystemTime.GameTime + 0.2;
			}
		}
		else
		{
			LadderValue = null;
			m_componentLocomotion.m_ladderActivationTime = m_componentLocomotion.m_subsystemTime.GameTime + 0.2;
		}
		if (m_componentLocomotion.JumpOrder > 0f)
		{
			m_componentLocomotion.m_componentCreature.ComponentCreatureSounds.PlayFootstepSound(2f);
			velocity += m_componentLocomotion.JumpSpeed * m_componentLocomotion.m_componentCreature.ComponentBody.Matrix.Forward;
			m_componentLocomotion.m_ladderActivationTime = m_componentLocomotion.m_subsystemTime.GameTime + 0.33;
			LadderValue = null;
			m_componentLocomotion.m_jumping = true;
		}
		if (m_componentLocomotion.IsCreativeFlyEnabled)
		{
			m_componentLocomotion.m_componentCreature.ComponentCreatureSounds.PlayFootstepSound(1f);
			LadderValue = null;
		}
		if (m_componentLocomotion.m_componentCreature.ComponentBody.ParentBody != null)
		{
			LadderValue = null;
		}
		m_componentLocomotion.m_componentCreature.ComponentBody.Velocity = velocity;
	}
}
