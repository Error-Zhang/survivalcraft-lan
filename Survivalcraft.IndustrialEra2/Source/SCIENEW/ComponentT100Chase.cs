using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentT100Chase : Component, IUpdateable, IDetectableComponent
{
	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemProjectiles m_subsystemProjectiles;

	public SubsystemAudio m_subsystemAudio;

	public SubsystemParticles m_subsystemParticles;

	public ComponentChaseBehavior m_componentChaseBehavior;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemProjectiles = Project.FindSubsystem<SubsystemProjectiles>(throwOnError: true);
		m_subsystemAudio = Project.FindSubsystem<SubsystemAudio>(throwOnError: true);
		m_subsystemParticles = Project.FindSubsystem<SubsystemParticles>(throwOnError: true);
		m_componentChaseBehavior = Entity.FindComponent<ComponentChaseBehavior>(throwOnError: true);
	}

	public void Update(float dt)
	{
		if (!m_componentChaseBehavior.IsActive || m_componentChaseBehavior.m_target == null || !IsTargetInAttackRange2(m_componentChaseBehavior.m_target.ComponentBody) || !m_componentChaseBehavior.m_subsystemTime.PeriodicGameTimeEvent(0.2, 0.0))
		{
			return;
		}
		m_componentChaseBehavior.m_componentCreatureModel.AttackOrder = false;
		BoundingBox boundingBox = m_componentChaseBehavior.m_componentCreature.ComponentBody.BoundingBox;
		BoundingBox boundingBox2 = m_componentChaseBehavior.m_target.ComponentBody.BoundingBox;
		Vector3 vector = 0.5f * (boundingBox.Min + boundingBox.Max);
		Vector3 vector2 = 0.5f * (boundingBox2.Min + boundingBox2.Max) - vector;
		if (vector2.Length() >= 10f)
		{
			Projectile projectile = m_subsystemProjectiles.FireProjectile(ItemsBlockManager.GetItemID("AntiTankBullet_Head"), vector + vector2.Normalize() * 1.5f, vector2.Normalize() * 200f, Vector3.Zero, m_componentChaseBehavior.m_componentCreature);
			if (projectile != null)
			{
				projectile.ProjectileStoppedAction = ProjectileStoppedAction.Disappear;
			}
		}
		if (vector2.Length() < 10f)
		{
			for (int i = 0; i < 12; i++)
			{
				Vector3 vector3 = m_componentChaseBehavior.m_random.Float(-0.01f, 0.01f) * Vector3.UnitX + m_componentChaseBehavior.m_random.Float(-0.01f, 0.01f) * Vector3.UnitY + m_componentChaseBehavior.m_random.Float(-0.01f, 0.01f) * Vector3.UnitZ;
				Projectile projectile2 = m_subsystemProjectiles.FireProjectile(ItemsBlockManager.GetItemID("LeadBullet_ShotGun"), vector + vector2.Normalize() * 1.5f, (vector2.Normalize() + vector3) * 200f, Vector3.Zero, m_componentChaseBehavior.m_componentCreature);
				if (projectile2 != null)
				{
					projectile2.ProjectileStoppedAction = ProjectileStoppedAction.Disappear;
				}
			}
		}
		m_subsystemAudio.PlaySound("Audio/MusketFire", 1f, 0f, vector, 20f, autoDelay: true);
		m_subsystemParticles.AddParticleSystem(new GunSmokeParticleSystem2(m_subsystemTerrain, vector + Vector3.Normalize(vector2) * 1f + new Vector3(0f, 0.5f, 0f), Vector3.Normalize(vector2)));
	}

	public bool IsTargetInAttackRange2(ComponentBody target, bool flag = false)
	{
		if (m_componentChaseBehavior.IsBodyInAttackRange(target))
		{
			return true;
		}
		BoundingBox boundingBox = m_componentChaseBehavior.m_componentCreature.ComponentBody.BoundingBox;
		BoundingBox boundingBox2 = target.BoundingBox;
		Vector3 vector = 0.5f * (boundingBox.Min + boundingBox.Max);
		float num = (0.5f * (boundingBox2.Min + boundingBox2.Max) - vector).Length();
		float num2 = 1f;
		if (flag)
		{
			num2 = 4f;
		}
		if (num < 30f * num2 && !m_subsystemTerrain.Raycast(vector, 0.5f * (boundingBox2.Min + boundingBox2.Max), useInteractionBoxes: false, skipAirBlocks: true, (int value, float distance) => Terrain.ExtractContents(value) != 0 && Terrain.ExtractContents(value) != WaterBlock.Index && Terrain.ExtractContents(value) != TallGrassBlock.Index && Terrain.ExtractContents(value) != GlassBlock.Index && Terrain.ExtractContents(value) != WoodenDoorBlock.Index && Terrain.ExtractContents(value) != WoodenSlabBlock.Index).HasValue)
		{
			return true;
		}
		return false;
	}

	public Vector2 GetPosition()
	{
		return m_componentChaseBehavior.m_componentCreature.ComponentBody.Position.XZ;
	}
}
