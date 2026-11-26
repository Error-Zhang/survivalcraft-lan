using System;
using System.Linq;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentDeviceRider : Component, IUpdateable
{
	public SubsystemBodies m_subsystemBodies;

	public SubsystemTerrain m_subsystemTerrain;

	public DynamicArray<ComponentBody> m_componentBodies = new DynamicArray<ComponentBody>();

	public Vector3 m_riderOffset;

	public float m_animationTime;

	public bool m_isAnimating;

	public bool m_isDismounting;

	public Vector3 m_targetPositionOffset;

	public Quaternion m_targetRotationOffset;

	public float m_outOfMountTime;

	public ComponentBody ComponentBody { get; set; }

	public ComponentMount Mount
	{
		get
		{
			if (ComponentBody.ParentBody != null)
			{
				return ComponentBody.ParentBody.Entity.FindComponent<ComponentMount>();
			}
			return null;
		}
	}

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public virtual ComponentMount FindNearestMount()
	{
		Vector2 point = new Vector2(ComponentBody.Position.X, ComponentBody.Position.Z);
		m_componentBodies.Clear();
		m_subsystemBodies.FindBodiesAroundPoint(point, 2.5f, m_componentBodies);
		float num = 0f;
		ComponentMount result = null;
		foreach (ComponentMount item in from b in m_componentBodies
			select b.Entity.FindComponent<ComponentMount>() into m
			where m != null && m.Entity != Entity
			select m)
		{
			float num2 = ScoreMount(item, 2.5f);
			if (num2 > num)
			{
				num = num2;
				result = item;
			}
		}
		return result;
	}

	public virtual void StartMounting(ComponentMount componentMount)
	{
		if (!m_isAnimating && Mount == null)
		{
			m_isAnimating = true;
			m_animationTime = 0f;
			m_isDismounting = false;
			ComponentBody.ParentBody = componentMount.ComponentBody;
			ComponentBody.ParentBodyPositionOffset = (ComponentBody.Position - componentMount.ComponentBody.Position).Transform(Quaternion.Conjugate(componentMount.ComponentBody.Rotation));
			ComponentBody.ParentBodyRotationOffset = Quaternion.Conjugate(componentMount.ComponentBody.Rotation) * ComponentBody.Rotation;
			m_targetPositionOffset = componentMount.MountOffset + m_riderOffset;
			m_targetRotationOffset = Quaternion.Identity;
		}
	}

	public virtual void StartDismounting()
	{
		if (!m_isAnimating && Mount != null)
		{
			float x = 0f;
			if (Mount.DismountOffset.X > 0f)
			{
				float num = Mount.DismountOffset.X + 0.5f;
				Vector3 vector = 0.5f * (ComponentBody.BoundingBox.Min + ComponentBody.BoundingBox.Max);
				TerrainRaycastResult? terrainRaycastResult = m_subsystemTerrain.Raycast(vector, vector - num * ComponentBody.Matrix.Right, useInteractionBoxes: false, skipAirBlocks: true, null);
				TerrainRaycastResult? terrainRaycastResult2 = m_subsystemTerrain.Raycast(vector, vector + num * ComponentBody.Matrix.Right, useInteractionBoxes: false, skipAirBlocks: true, null);
				if (terrainRaycastResult.HasValue)
				{
					if (terrainRaycastResult2.HasValue)
					{
						x = ((!(terrainRaycastResult.Value.Distance > terrainRaycastResult2.Value.Distance)) ? MathUtils.Min(terrainRaycastResult2.Value.Distance, Mount.DismountOffset.X) : (0f - MathUtils.Min(terrainRaycastResult.Value.Distance, Mount.DismountOffset.X)));
					}
					else
					{
						x = Mount.DismountOffset.X;
					}
				}
				else
				{
					x = 0f - Mount.DismountOffset.X;
				}
			}
			m_isAnimating = true;
			m_animationTime = 0f;
			m_isDismounting = true;
			m_targetPositionOffset = Mount.MountOffset + m_riderOffset + new Vector3(x, Mount.DismountOffset.Y, Mount.DismountOffset.Z);
			m_targetRotationOffset = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)MathF.Sign(x) * MathUtils.DegToRad(60f));
		}
	}

	public void Update(float dt)
	{
		if (m_isAnimating)
		{
			float num = 8f * dt;
			ComponentBody componentBody = ComponentBody;
			componentBody.ParentBodyPositionOffset = componentBody.ParentBodyPositionOffset.Lerp(m_targetPositionOffset, num);
			componentBody.ParentBodyRotationOffset = Quaternion.Slerp(componentBody.ParentBodyRotationOffset, m_targetRotationOffset, num);
			m_animationTime += dt;
			if (componentBody.ParentBodyPositionOffset.DistanceSquared(m_targetPositionOffset) < 0.010000001f || m_animationTime > 0.75f)
			{
				m_isAnimating = false;
				if (m_isDismounting)
				{
					if (componentBody.ParentBody != null)
					{
						componentBody.Velocity = componentBody.ParentBody.Velocity;
						componentBody.ParentBody = null;
					}
				}
				else
				{
					componentBody.ParentBodyPositionOffset = m_targetPositionOffset;
					componentBody.ParentBodyRotationOffset = m_targetRotationOffset;
					m_outOfMountTime = 0f;
				}
			}
		}
		ComponentMount mount = Mount;
		if (mount != null && !m_isAnimating)
		{
			ComponentBody componentBody2 = ComponentBody;
			ComponentBody parentBody = ComponentBody.ParentBody;
			if ((parentBody.Position + componentBody2.ParentBodyPositionOffset.Transform(parentBody.Rotation)).DistanceSquared(componentBody2.Position) > 0.16000001f)
			{
				m_outOfMountTime += dt;
			}
			else
			{
				m_outOfMountTime = 0f;
			}
			ComponentHealth componentHealth = mount.Entity.FindComponent<ComponentHealth>();
			if (m_outOfMountTime > 0.1f || (componentHealth != null && componentHealth.Health <= 0f))
			{
				StartDismounting();
			}
			ComponentBody.ParentBodyPositionOffset = mount.MountOffset + m_riderOffset;
			ComponentBody.ParentBodyRotationOffset = Quaternion.Identity;
		}
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		m_subsystemBodies = Project.FindSubsystem<SubsystemBodies>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		ComponentBody = Entity.FindComponent<ComponentBody>(throwOnError: true);
		m_riderOffset = valuesDictionary.GetValue<Vector3>("RiderOffset");
	}

	public virtual float ScoreMount(ComponentMount componentMount, float maxDistance)
	{
		if (componentMount.ComponentBody.Velocity.LengthSquared() < 1f)
		{
			Vector3 vector = componentMount.ComponentBody.Position + componentMount.MountOffset.Transform(componentMount.ComponentBody.Rotation) - ComponentBody.Position;
			if (vector.Length() < maxDistance)
			{
				Vector3 forward = Matrix.CreateFromQuaternion(ComponentBody.Rotation).Forward;
				if (vector.Normalize().Dot(forward) > 0.33f)
				{
					return maxDistance - vector.Length();
				}
			}
		}
		return 0f;
	}
}
