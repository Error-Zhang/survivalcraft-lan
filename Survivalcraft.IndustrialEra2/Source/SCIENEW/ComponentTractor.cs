using System;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentTractor : ComponentBoat, IUpdateable
{
	private ComponentCombustionEngine componentEngine;

	public ComponentPropellerModel m_componentPropellerModel;

	public SubsystemTerrain m_subsystemTerrain;

	private float m_propellerRotateSpeed;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		componentEngine = Entity.FindComponent<ComponentCombustionEngine>();
		m_componentPropellerModel = Entity.FindComponent<ComponentPropellerModel>();
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>();
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
		if (MoveOrder != 0f && rider != null && componentBody.StandingOnValue.HasValue && componentEngine != null && componentEngine.HeatLevel > 0f)
		{
			m_componentBody.Velocity += dt * 40f * MoveOrder * m_componentBody.Matrix.Forward;
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
		if (componentEngine.HeatLevel <= 0f || componentBody.Mass > 1200f || !componentBody.StandingOnValue.HasValue)
		{
			return;
		}
		int num2 = -1;
		int num3 = 0;
		for (int i = 0; i < 6; i++)
		{
			if (componentEngine.GetSlotValue(i) == SaltpeterChunkBlock.Index && componentEngine.GetSlotCount(i) > 0)
			{
				num2 = i;
			}
		}
		if (num2 >= 0 && num3 == 0)
		{
			componentEngine.m_slots[num2].Count--;
			num3 = 9;
		}
		int value = Terrain.ExtractContents(componentBody.StandingOnValue.Value);
		Vector3 vector = componentBody.Rotation.GetForwardVector().Normalize();
		Vector3 vector2 = vector.Transform(Matrix.CreateRotationY((float)Math.PI / 2f));
		componentBody.IsSneaking = true;
		for (int j = -3; j < 4; j++)
		{
			Point3 point = Terrain.ToCell(componentBody.Position - vector * 2f + j * vector2 + new Vector3(0f, 0.1f, 0f));
			int cellContentsFast = m_subsystemTerrain.Terrain.GetCellContentsFast(point.X, point.Y - 1, point.Z);
			if (cellContentsFast != DirtBlock.Index && cellContentsFast != GrassBlock.Index && cellContentsFast != SoilBlock.Index)
			{
				continue;
			}
			if (m_subsystemTerrain.Terrain.GetCellContentsFast(point.X, point.Y, point.Z) == 0)
			{
				for (int k = 0; k < 6; k++)
				{
					int num4 = 0;
					switch (componentEngine.GetSlotValue(k))
					{
					case 16557:
						num4 = 0x14 | (FlowerBlock.SetIsSmall(0, isSmall: true) << 14);
						break;
					case 173:
						num4 = 0x13 | (TallGrassBlock.SetIsSmall(0, isSmall: true) << 14);
						break;
					case 49325:
						num4 = 0x19 | (FlowerBlock.SetIsSmall(0, isSmall: true) << 14);
						break;
					case 32941:
						num4 = 0x18 | (FlowerBlock.SetIsSmall(0, isSmall: true) << 14);
						break;
					case 82093:
						num4 = 0xAE | (RyeBlock.SetSize(RyeBlock.SetIsWild(0, isWild: false), 0) << 14);
						break;
					case 65709:
						num4 = 0xAE | (RyeBlock.SetSize(RyeBlock.SetIsWild(0, isWild: false), 0) << 14);
						break;
					case 114861:
						num4 = 0x83 | (BasePumpkinBlock.SetSize(BasePumpkinBlock.SetIsDead(0, isDead: false), 0) << 14);
						break;
					case 98477:
						num4 = 0xCC | (CottonBlock.SetSize(CottonBlock.SetIsWild(0, isWild: false), 0) << 14);
						break;
					}
					if (num4 > 0 && componentEngine.GetSlotCount(k) > 0 && m_subsystemTerrain.Terrain.GetCellContentsFast(point.X, point.Y, point.Z) == 0)
					{
						componentEngine.m_slots[k].Count--;
						m_subsystemTerrain.ChangeCell(point.X, point.Y, point.Z, num4);
						k += 6;
					}
				}
			}
			if (num3 > 0 && m_subsystemTerrain.Terrain.GetCellValueFast(point.X, point.Y - 1, point.Z) < 98472 && cellContentsFast == SoilBlock.Index)
			{
				m_subsystemTerrain.ChangeCell(point.X, point.Y - 1, point.Z, 98472);
				num3--;
			}
			else if (cellContentsFast != SoilBlock.Index)
			{
				m_subsystemTerrain.ChangeCell(point.X, point.Y - 1, point.Z, Terrain.ReplaceContents(value, 168));
			}
		}
	}
}
