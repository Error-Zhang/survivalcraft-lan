using System;
using System.Linq;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentTrain : Component, IUpdateable
{
	private static readonly Vector3 center = new Vector3(0.5f, 0f, 0.5f);

	private static readonly Quaternion[] directions = new Quaternion[4]
	{
		Quaternion.CreateFromAxisAngle(Vector3.UnitY, 0f),
		Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)Math.PI / 2f),
		Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)Math.PI),
		Quaternion.CreateFromAxisAngle(Vector3.UnitY, 4.712389f)
	};

	private static readonly Quaternion upwardDirection = Quaternion.CreateFromAxisAngle(new Vector3(1f, 0f, 0f), (float)Math.PI / 4f);

	private static readonly Quaternion downwardDirection = Quaternion.CreateFromAxisAngle(new Vector3(1f, 0f, 0f), -(float)Math.PI / 4f);

	private static readonly Vector3[] forwardVectors = new Vector3[4]
	{
		new Vector3(0f, 0f, -1f),
		new Vector3(-1f, 0f, 0f),
		new Vector3(0f, 0f, 1f),
		new Vector3(1f, 0f, 0f)
	};

	internal ComponentBody m_componentBody;

	public ComponentTrain ParentBody;

	protected ComponentDamage componentDamage;

	protected float m_outOfMountTime;

	private ComponentMount m_componentMount;

	public ComponentSteamEngineV ComponentEngine;

	private int m_forwardDirection;

	private Quaternion rotation;

	private Vector3 forwardVector;

	private Vector3 m_previousPosition;

	private Vector3 m_realSpeed;

	public SubsystemBodies m_subsystemBodies;

	public SubsystemTerrain m_subsystemTerrain;

	public int Direction
	{
		get
		{
			return m_forwardDirection;
		}
		set
		{
			forwardVector = forwardVectors[value];
			m_forwardDirection = value;
			rotation = directions[value];
		}
	}

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_subsystemBodies = Project.FindSubsystem<SubsystemBodies>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_componentBody = Entity.FindComponent<ComponentBody>(throwOnError: true);
		componentDamage = Entity.FindComponent<ComponentDamage>();
		m_componentMount = Entity.FindComponent<ComponentMount>(throwOnError: true);
		if ((ComponentEngine = Entity.FindComponent<ComponentSteamEngineV>()) != null)
		{
			ComponentBody componentBody = m_componentBody;
			componentBody.CollidedWithBody = (Action<ComponentBody>)Delegate.Combine(componentBody.CollidedWithBody, new Action<ComponentBody>(CollidedWithBody));
		}
		else
		{
			ParentBody = valuesDictionary.GetValue<EntityReference>("ParentBody", default).GetComponent<ComponentTrain>(Entity, idToEntityMap, throwIfNotFound: false);
		}
		Direction = valuesDictionary.GetValue("Direction", 0);
		m_forwardDirection = Direction;
		m_previousPosition = m_componentBody.Position;
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		if (m_forwardDirection != 0)
		{
			valuesDictionary.SetValue("Direction", m_forwardDirection);
		}
		EntityReference value = EntityReference.FromId(ParentBody, entityToIdMap);
		if (!value.IsNullOrEmpty())
		{
			valuesDictionary.SetValue("ParentBody", value);
		}
	}

	public void CollidedWithBody(ComponentBody body)
	{
		if (Math.Abs(body.Density - 9.76f) <= float.Epsilon || m_componentBody.m_velocity.Length() < 0.5f)
		{
			return;
		}
		float num = (m_realSpeed.XZ - body.Velocity.XZ).LengthSquared() * 0.3f;
		if (!(num < 0.02f) && m_realSpeed.XZ.LengthSquared() != 0f)
		{
			ComponentHealth componentHealth = body.Entity.FindComponent<ComponentHealth>();
			if (componentHealth != null)
			{
				componentHealth.Injure(num / componentHealth.AttackResilience, null, ignoreInvulnerability: false, "Struck by a train");
			}
			else
			{
				body.Entity.FindComponent<ComponentDamage>()?.Damage(num);
			}
			body.ApplyImpulse(Math.Clamp(7.5f * MathF.Pow(m_componentBody.Mass / body.Mass, 0.33f), 0f, 6f) * (body.Position - m_componentBody.Position).Normalize());
		}
	}

	public ComponentTrain FindNearestTrain()
	{
		DynamicArray<ComponentBody> dynamicArray = new DynamicArray<ComponentBody>();
		m_subsystemBodies.FindBodiesAroundPoint(m_componentBody.Position.XZ, 2f, dynamicArray);
		float num = 0f;
		ComponentTrain result = null;
		foreach (ComponentTrain item in dynamicArray.Select(GetRailEntity))
		{
			if (item == null || item.Entity == Entity)
			{
				continue;
			}
			float num2 = 0f;
			if (item.m_componentBody.Velocity.LengthSquared() < 1f && item.Direction == Direction)
			{
				Vector3 vector = item.m_componentBody.Position + item.m_componentMount.MountOffset.Transform(item.m_componentBody.Rotation) - m_componentBody.Position;
				if (vector.LengthSquared() <= 4f)
				{
					num2 = 4f - vector.LengthSquared();
				}
			}
			if (num2 > num)
			{
				num = num2;
				result = item;
			}
		}
		return result;
	}

	public void SetDirection(int value)
	{
		Direction = value;
		m_componentBody.Rotation = rotation;
	}

	public void Update(float dt)
	{
		if (m_componentMount.Rider != null)
		{
			ComponentPlayer componentPlayer = m_componentMount.Rider.Entity.FindComponent<ComponentPlayer>(throwOnError: true);
			componentPlayer.ComponentLocomotion.LookOrder = componentPlayer.ComponentInput.PlayerInput.Look;
		}
		switch (Direction)
		{
		case 0:
		case 2:
			m_componentBody.Position = new Vector3(MathF.Floor(m_componentBody.Position.X) + 0.5f, m_componentBody.Position.Y, m_componentBody.Position.Z);
			break;
		case 1:
		case 3:
			m_componentBody.Position = new Vector3(m_componentBody.Position.X, m_componentBody.Position.Y, MathF.Floor(m_componentBody.Position.Z) + 0.5f);
			break;
		}
		ComponentTrain componentTrain = this;
		int num = 0;
		while (componentTrain.ParentBody != null)
		{
			componentTrain = componentTrain.ParentBody;
			num++;
		}
		if (num > 0)
		{
			ComponentBody componentBody = componentTrain.m_componentBody;
			_ = componentBody.Position;
			_ = componentBody.Rotation;
			ComponentSteamEngineV componentSteamEngineV = componentBody.Entity.FindComponent<ComponentSteamEngineV>();
			float num2 = 0f;
			if (componentSteamEngineV != null)
			{
				num2 = (componentSteamEngineV.IsPowering() ? 100f : 1f);
			}
			if (m_componentBody.StandingOnValue.HasValue && num2 >= 100f && componentBody.m_velocity.Length() > 0f)
			{
				TerrainRaycastResult? terrainRaycastResult = m_subsystemTerrain.Raycast(m_componentBody.Position, m_componentBody.Position + new Vector3(0f, -3f, 0f), useInteractionBoxes: false, skipAirBlocks: true, null);
				if (terrainRaycastResult.HasValue && Terrain.ExtractContents(terrainRaycastResult.Value.Value) == RailBlock.Index && (dt *= SimulateRail(RailBlock.GetRailType(Terrain.ExtractData(terrainRaycastResult.Value.Value)))) > 0f)
				{
					float num3 = 6f;
					if (num2 >= 200f)
					{
						num3 = 30f;
					}
					m_componentBody.m_velocity = num3 * rotation.GetForwardVector();
					if (ParentBody != null && MathF.Abs(ParentBody.m_componentBody.Position.Distance(m_componentBody.Position)) > 2f)
					{
						m_componentBody.m_velocity = num3 * rotation.GetForwardVector() * 1.4f;
					}
					if (ParentBody != null && MathF.Abs(ParentBody.m_componentBody.Position.Distance(m_componentBody.Position)) < 1.2f)
					{
						m_componentBody.m_velocity = num3 * rotation.GetForwardVector() * 0.3f;
					}
					if (ParentBody != null && MathF.Abs(ParentBody.m_componentBody.Position.Distance(m_componentBody.Position)) < 0.8f)
					{
						m_componentBody.m_velocity = num3 * rotation.GetForwardVector() * 0f;
					}
					m_componentBody.Rotation = Quaternion.Slerp(m_componentBody.Rotation, rotation, 0.715f);
				}
			}
			else if (componentSteamEngineV != null && num2 < 100f && m_componentBody.m_velocity.Length() - 0.06f > 0f && m_componentBody.StandingOnValue.HasValue)
			{
				SimulateRail(RailBlock.GetRailType(Terrain.ExtractData(m_subsystemTerrain.Raycast(m_componentBody.Position, m_componentBody.Position + new Vector3(0f, -3f, 0f), useInteractionBoxes: false, skipAirBlocks: true, null).GetValueOrDefault().Value)));
				m_componentBody.m_velocity = (m_componentBody.m_velocity.Length() - 0.06f) * rotation.GetForwardVector();
			}
			if (!m_componentBody.StandingOnValue.HasValue)
			{
				SimulateRail(RailBlock.GetRailType(Terrain.ExtractData(m_subsystemTerrain.Raycast(m_componentBody.Position, m_componentBody.Position + new Vector3(0f, -3f, 0f), useInteractionBoxes: false, skipAirBlocks: true, null).GetValueOrDefault().Value)));
				m_componentBody.Rotation = rotation;
				m_componentBody.m_velocity -= new Vector3(0f, 40f, 0f) * dt;
			}
			m_componentBody.Rotation = Quaternion.Slerp(m_componentBody.Rotation, rotation, 0.715f);
			m_outOfMountTime = ((MathF.Abs(ParentBody.m_componentBody.Position.DistanceSquared(m_componentBody.Position)) > 24f) ? (m_outOfMountTime + dt) : 0f);
			ComponentDamage componentDamage = ParentBody.Entity.FindComponent<ComponentDamage>();
			if (!(m_outOfMountTime > 12f))
			{
				ComponentDamage componentDamage2 = this.componentDamage;
				if ((componentDamage2 == null || !(componentDamage2.Hitpoints <= 0.05f)) && (componentDamage == null || !(componentDamage.Hitpoints <= 0.05f)))
				{
					goto IL_05f2;
				}
			}
			ParentBody = null;
			goto IL_05f2;
		}
		if (!m_componentBody.StandingOnValue.HasValue)
		{
			SimulateRail(RailBlock.GetRailType(Terrain.ExtractData(m_subsystemTerrain.Raycast(m_componentBody.Position, m_componentBody.Position + new Vector3(0f, -3f, 0f), useInteractionBoxes: false, skipAirBlocks: true, null).GetValueOrDefault().Value)));
			m_componentBody.Rotation = rotation;
			m_componentBody.m_velocity -= new Vector3(0f, 40f, 0f) * dt;
		}
		ComponentSteamEngineV componentSteamEngineV2 = m_componentBody.Entity.FindComponent<ComponentSteamEngineV>();
		float num4 = 0f;
		if (componentSteamEngineV2 != null)
		{
			num4 = (componentSteamEngineV2.IsPowering() ? 100f : 1f);
		}
		if (num4 >= 100f && m_componentBody.StandingOnValue.HasValue)
		{
			float num5 = 6f;
			if (num4 >= 200f)
			{
				num5 = 30f;
			}
			TerrainRaycastResult valueOrDefault = m_subsystemTerrain.Raycast(m_componentBody.Position, m_componentBody.Position + new Vector3(0f, -3f, 0f), useInteractionBoxes: false, skipAirBlocks: true, null).GetValueOrDefault();
			if (Terrain.ExtractContents(valueOrDefault.Value) == RailBlock.Index && (dt *= SimulateRail(RailBlock.GetRailType(Terrain.ExtractData(valueOrDefault.Value)))) > 0f)
			{
				m_componentBody.m_velocity = num5 * rotation.GetForwardVector();
			}
		}
		else if (num4 < 100f && m_componentBody.m_velocity.Length() - 0.06f > 0f && m_componentBody.StandingOnValue.HasValue)
		{
			SimulateRail(RailBlock.GetRailType(Terrain.ExtractData(m_subsystemTerrain.Raycast(m_componentBody.Position, m_componentBody.Position + new Vector3(0f, -3f, 0f), useInteractionBoxes: false, skipAirBlocks: true, null).GetValueOrDefault().Value)));
			m_componentBody.m_velocity = (m_componentBody.m_velocity.Length() - 0.06f) * rotation.GetForwardVector();
		}
		m_componentBody.Rotation = Quaternion.Slerp(m_componentBody.Rotation, rotation, 0.715f);
		UpdateSpeed(dt);
		return;
		IL_05f2:
		UpdateSpeed(dt);
	}

	private void UpdateSpeed(float dt)
	{
		if (m_previousPosition != m_componentBody.Position)
		{
			Vector3 vector = new Vector3(MathF.Abs(m_componentBody.Position.X - m_previousPosition.X), MathF.Abs(m_componentBody.Position.Y - m_previousPosition.Y), MathF.Abs(m_componentBody.Position.Z - m_previousPosition.Z));
			m_realSpeed = vector / dt;
			m_previousPosition = m_componentBody.Position;
		}
	}

	private float SimulateRail(int railType)
	{
		if (RailBlock.IsCorner(railType))
		{
			if (GetOffsetOnDirection(m_componentBody.Position, m_forwardDirection) > 0.5f)
			{
				Turn(railType);
			}
			return 50f;
		}
		if (RailBlock.IsDirectionX(railType) ^ !RailBlock.IsDirectionX(m_forwardDirection))
		{
			ref Quaternion reference = ref rotation;
			Quaternion quaternion;
			if (railType <= 5)
			{
				quaternion = directions[Direction];
			}
			else
			{
				quaternion = ((railType - 6 != Direction) ? (directions[Direction] * upwardDirection) : (directions[Direction] * downwardDirection));
			}
			reference = quaternion;
			if (railType <= 5 || railType - 6 == Direction)
			{
				return 50f;
			}
			return 30f;
		}
		return 0f;
	}

	private bool Turn(int turnType)
	{
		if (Direction == turnType)
		{
			Direction = (Direction - 1) & 3;
			m_componentBody.Velocity = MathF.Abs(m_componentBody.Velocity.X + m_componentBody.Velocity.Z) * forwardVector;
			m_componentBody.Position = m_componentBody.Position.Floor() + center;
			return true;
		}
		if (((Direction - 1) & 3) == turnType)
		{
			Direction = (Direction + 1) & 3;
			m_componentBody.Velocity = MathF.Abs(m_componentBody.Velocity.X + m_componentBody.Velocity.Z) * forwardVector;
			m_componentBody.Position = m_componentBody.Position.Floor() + center;
			return true;
		}
		return false;
	}

	private static float GetOffsetOnDirection(Vector3 vec, int direction)
	{
		float num = (((direction & 1) == 0) ? (vec.Z - MathF.Floor(vec.Z)) : (vec.X - MathF.Floor(vec.X)));
		if ((direction & 2) != 0)
		{
			return num;
		}
		return 1f - num;
	}

	public static ComponentTrain GetRailEntity(Component c)
	{
		return c.Entity.FindComponent<ComponentTrain>();
	}
}
