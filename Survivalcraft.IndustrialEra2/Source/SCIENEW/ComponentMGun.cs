using System;
using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentMGun : ComponentBoat, IUpdateable
{
	private ComponentChest m_componentChest;

	public new SubsystemTime m_subsystemTime;

	public SubsystemProjectiles m_subsystemProjectiles;

	public new SubsystemAudio m_subsystemAudio;

	public SubsystemTerrain m_subsystemTerrain;

	public int fire;

	public Vector3 vect;

	public Game.Random m_random;

	private Dictionary<string, string> m_bulletMap = new Dictionary<string, string>();

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentChest = Entity.FindComponent<ComponentChest>();
		m_subsystemTime = Project.FindSubsystem<SubsystemTime>();
		m_subsystemProjectiles = Project.FindSubsystem<SubsystemProjectiles>();
		m_subsystemAudio = Project.FindSubsystem<SubsystemAudio>();
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>();
		m_random = new Game.Random();
		m_bulletMap.Add("FixedBullet", "IronBullet");
		m_bulletMap.Add("StandardBullet", "LeadBullet");
		m_bulletMap.Add("AntiTankBullet", "AntiTankBullet_Head");
	}

	public new void Update(float dt)
	{
		if (m_componentChest != null && m_subsystemTime.PeriodicGameTimeEvent(0.11, 0.0) && fire > 0)
		{
			int index = -1;
			int num = 0;
			string value = string.Empty;
			for (int i = 0; i < 16; i++)
			{
				int slotValue = m_componentChest.GetSlotValue(i);
				if (Terrain.ExtractContents(slotValue) == BaseItemBlock.Index && m_bulletMap.TryGetValue(ItemsBlockManager.GetItemCraftingID(slotValue), out value))
				{
					index = i;
					num = -1;
					break;
				}
			}
			if (num == -1)
			{
				new Vector3(m_componentMount.Rider.ComponentCreature.ComponentCreatureModel.EyeRotation.GetForwardVector().X, Math.Clamp(m_componentMount.Rider.ComponentCreature.ComponentCreatureModel.EyeRotation.GetForwardVector().Y, -0.05f, 0.05f), m_componentMount.Rider.ComponentCreature.ComponentCreatureModel.EyeRotation.GetForwardVector().Z);
				Vector3 vector = new Vector3(vect.X, Math.Clamp(vect.Y, -0.2f, 0.5f), vect.Z);
				Vector3 vector2 = new Vector3(m_random.Float(-10f, 10f), m_random.Float(-10f, 10f), m_random.Float(-10f, 10f));
				m_subsystemProjectiles.AddProjectile(ItemsBlockManager.GetItemID(value), m_componentBody.Position + vector.Normalize() * 1.8f + new Vector3(0f, 1.5f, 0f), vector.Normalize() * 320f + vector2, Vector3.Zero, m_componentMount.Rider.ComponentCreature);
				m_subsystemAudio.PlaySound("Audio/MusketFire", 1f, 0f, m_componentBody.Position, 10f, autoDelay: true);
				m_componentBody.m_subsystemParticles.AddParticleSystem(new GunSmokeParticleSystem2(m_subsystemTerrain, m_componentBody.Position + new Vector3(0f, 1f, 0f) * 1.4f + m_componentBody.Matrix.Forward * 1f, m_componentBody.Matrix.Forward));
				m_componentChest.m_slots[index].Count--;
			}
			fire = 0;
		}
		_ = m_componentBody.ImmersionFactor;
		if (!(m_componentBody.ImmersionFactor > 0.95f) && m_componentBody.ImmersionFactor > 0.01f && !m_componentBody.StandingOnValue.HasValue)
		{
			_ = m_componentBody.StandingOnBody == null;
		}
		else
			_ = 0;
		m_turnSpeed += 2.5f * m_subsystemTime.GameTimeDelta * (1f * TurnOrder - m_turnSpeed);
		Quaternion rotation = m_componentBody.Rotation;
		float num2 = MathF.Atan2(2f * rotation.Y * rotation.W - 2f * rotation.X * rotation.Z, 1f - 2f * rotation.Y * rotation.Y - 2f * rotation.Z * rotation.Z);
		if (3f != 0f)
		{
			num2 -= m_turnSpeed * dt;
		}
		m_componentBody.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, num2);
		MoveOrder = 0f;
		TurnOrder = 0f;
	}
}
