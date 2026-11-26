using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions;
using SCIENEW.VoltNet;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentAutoGun : ComponentInventoryBase, IUpdateable
{
	public SubsystemTime m_subsystemTime;

	public SubsystemProjectiles m_subsystemProjectiles;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemAudio m_subsystemAudio;

	public SubsystemParticles m_subsystemParticles;

	public SubsystemVoltNet m_subsystemVoltNet;

	public ComponentBlockEntity m_componentBlockEntity;

	public ComponentIEModel m_componentIEModel;

	public ComponentTurretTargeting m_componentTurretTargeting;

	private Dictionary<string, string> m_bulletMap = new Dictionary<string, string>();

	private new Game.Random m_random;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public Vector3 TargetDirection { get; protected set; } = Vector3.UnitZ;

	public Vector3 CurrentDirection { get; protected set; } = Vector3.UnitZ;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
		m_subsystemProjectiles = Project.FindSubsystem<SubsystemProjectiles>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemParticles = Project.FindSubsystem<SubsystemParticles>(throwOnError: true);
		m_subsystemAudio = Project.FindSubsystem<SubsystemAudio>(throwOnError: true);
		m_subsystemVoltNet = Project.FindSubsystem<SubsystemVoltNet>(throwOnError: true);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_componentIEModel = Entity.FindComponent<ComponentIEModel>(throwOnError: true);
		m_componentTurretTargeting = Entity.FindComponent<ComponentTurretTargeting>(throwOnError: true);
		m_random = new Game.Random();
		m_bulletMap.Add("FixedBullet", "IronBullet");
		m_bulletMap.Add("StandardBullet", "LeadBullet");
		m_bulletMap.Add("AntiTankBullet", "AntiTankBullet_Head");
		TargetDirection = valuesDictionary.GetValue("TargetDirection", Vector3.UnitZ);
		CurrentDirection = valuesDictionary.GetValue("CurrentDirection", Vector3.UnitZ);
		AutoGunDevice device = DevicesBlockManager.GetDevice<AutoGunDevice>();
		if (device != null)
		{
			m_componentIEModel.SetModel(device.m_model);
			m_componentIEModel.SetTexture(device.m_texture);
			m_componentIEModel.MainBoneName = m_componentIEModel.Model.RootBone.Name;
		}
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
		valuesDictionary.SetValue("TargetDirection", TargetDirection);
		valuesDictionary.SetValue("CurrentDirection", CurrentDirection);
	}

	public override int GetSlotCapacity(int slotIndex, int slotValue)
	{
		if (Terrain.ExtractContents(slotValue) != BaseItemBlock.Index || !m_bulletMap.TryGetValue(ItemsBlockManager.GetItemCraftingID(slotValue), out string _))
		{
			return 0;
		}
		return base.GetSlotCapacity(slotIndex, slotValue);
	}

	public void Update(float dt)
	{
		VoltElement voltElement = m_subsystemVoltNet.GetVoltElement(m_componentBlockEntity.Coordinates);
		if (voltElement is AutoGunVoltElement && voltElement.CurrentWorkState == WorkState.Active)
		{
			UpdateFiring(dt);
		}
		UpdateDrawing(dt);
	}

	private void UpdateFiring(float dt)
	{
		if (!m_subsystemTime.PeriodicGameTimeEvent(0.14, 0.0))
		{
			return;
		}
		int slotIndex = -1;
		bool flag = false;
		int value = 0;
		for (int i = 0; i < m_slots.Count; i++)
		{
			int slotValue = GetSlotValue(i);
			if (Terrain.ExtractContents(slotValue) == BaseItemBlock.Index && m_bulletMap.TryGetValue(ItemsBlockManager.GetItemCraftingID(slotValue), out string value2))
			{
				slotIndex = i;
				flag = true;
				value = ItemsBlockManager.GetItemID(value2);
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		Vector3 vector = new Vector3(coordinates.X, coordinates.Y, coordinates.Z) + new Vector3(0.5f);
		if (m_componentTurretTargeting.TryAcquireTarget(vector, 64f, out ComponentBody targetBody, out Vector3 aimDirection))
		{
			TargetDirection = aimDirection;
			Vector3 vector2 = Vector3.Normalize(aimDirection);
			Vector3 vector3 = vector + vector2 * 1f;
			if (m_componentTurretTargeting.CanFireSafely(vector3, vector2, targetBody, 50f))
			{
				m_subsystemProjectiles.FireProjectile(value, vector3, vector2 * 260f, Vector3.Zero, null);
				m_subsystemAudio.PlaySound("Audio/MusketFire", 1f, 0f, new Vector3((float)coordinates.X + 0.5f, (float)coordinates.Y + 0.5f, (float)coordinates.Z + 0.5f), 10f, autoDelay: true);
				RemoveSlotItems(slotIndex, 1);
				m_subsystemParticles.AddParticleSystem(new GunSmokeParticleSystem2(m_subsystemTerrain, vector3, aimDirection));
			}
		}
	}

	private void UpdateDrawing(float dt)
	{
		Matrix value = Matrix.CreateTranslation(new Vector3(m_componentBlockEntity.Coordinates) + new Vector3(0.5f, 0f, 0.5f));
		CurrentDirection = CurrentDirection.Slerp(TargetDirection, 30f * dt);
		Matrix value2 = Matrix.CreateLookAt(Vector3.Zero, CurrentDirection, Vector3.UnitY).Invert();
		m_componentIEModel.SetBoneTransform(m_componentIEModel.Model.RootBone.Index, value);
		m_componentIEModel.SetBoneTransform("AutoGun", value2);
	}
}
