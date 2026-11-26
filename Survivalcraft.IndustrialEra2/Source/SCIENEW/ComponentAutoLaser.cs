using System;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions;
using SCIENEW.VoltNet;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentAutoLaser : Component, IUpdateable
{
	public SubsystemTime m_subsystemTime;

	public SubsystemAudio m_subsystemAudio;

	public SubsystemVoltNet m_subsystemVoltNet;

	public SubsystemLaser m_subsystemLaser;

	public ComponentBlockEntity m_componentBlockEntity;

	public ComponentIEModel m_componentIEModel;

	public ComponentTurretTargeting m_componentTurretTargeting;

	public double m_energy;

	private float m_laserOffset;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public bool HasTarget { get; protected set; }

	public Vector3 TargetDirection { get; protected set; } = Vector3.UnitZ;

	public Vector3 CurrentDirection { get; protected set; } = Vector3.UnitZ;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
		m_subsystemAudio = Project.FindSubsystem<SubsystemAudio>(throwOnError: true);
		m_subsystemVoltNet = Project.FindSubsystem<SubsystemVoltNet>(throwOnError: true);
		m_subsystemLaser = Project.FindSubsystem<SubsystemLaser>(throwOnError: true);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_componentIEModel = Entity.FindComponent<ComponentIEModel>(throwOnError: true);
		m_componentTurretTargeting = Entity.FindComponent<ComponentTurretTargeting>(throwOnError: true);
		TargetDirection = valuesDictionary.GetValue("TargetDirection", Vector3.UnitZ);
		CurrentDirection = valuesDictionary.GetValue("CurrentDirection", Vector3.UnitZ);
		HasTarget = valuesDictionary.GetValue("HasTarget", defaultValue: false);
		m_laserOffset = MathF.Sqrt(2f) / 2f + 0.1f;
		AutoLaserDevice device = DevicesBlockManager.GetDevice<AutoLaserDevice>();
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
		valuesDictionary.SetValue("HasTarget", HasTarget);
	}

	public void Update(float dt)
	{
		if (m_energy > 0.0)
		{
			UpdateFocus(dt);
		}
		UpdateDrawing(dt);
		if (HasTarget && m_energy > 0.0)
		{
			UpdateShoot(dt);
		}
	}

	private void UpdateFocus(float dt)
	{
		if (m_subsystemTime.PeriodicGameTimeEvent(0.34, 0.0))
		{
			Point3 coordinates = m_componentBlockEntity.Coordinates;
			Vector3 origin = new Vector3(coordinates.X, coordinates.Y, coordinates.Z) + new Vector3(0.5f);
			HasTarget = m_componentTurretTargeting.TryAcquireTarget(origin, 64f, out ComponentBody _, out Vector3 aimDirection);
			if (HasTarget)
			{
				TargetDirection = Vector3.Normalize(aimDirection);
			}
		}
	}

	private void UpdateDrawing(float dt)
	{
		CurrentDirection = CurrentDirection.Slerp(TargetDirection, 30f * dt);
		Matrix value = Matrix.CreateTranslation(new Vector3(m_componentBlockEntity.Coordinates) + new Vector3(0.5f, 0f, 0.5f));
		Matrix value2 = Matrix.CreateLookAt(Vector3.Zero, CurrentDirection, Vector3.UnitY).Invert();
		m_componentIEModel.SetBoneTransform(m_componentIEModel.Model.RootBone.Index, value);
		m_componentIEModel.SetBoneTransform("Emitter", value2);
	}

	private void UpdateShoot(float dt)
	{
		if (!(CurrentDirection.Distance(TargetDirection) < 0.2f))
		{
			return;
		}
		Vector3 vector = new Vector3(m_componentBlockEntity.Coordinates) + new Vector3(0.5f);
		if (m_componentTurretTargeting.TryAcquireTarget(vector, 64f, out ComponentBody targetBody, out Vector3 aimDirection))
		{
			Vector3 vector2 = Vector3.Normalize(aimDirection);
			Vector3 vector3 = vector + vector2 * m_laserOffset;
			if (m_componentTurretTargeting.CanFireSafely(vector3, vector2, targetBody, 50f))
			{
				m_subsystemLaser.Spawn(vector3, vector + vector2 * 0.51f, vector2, Color.Red * new Color(255, 255, 255, 128), m_energy);
			}
		}
	}
}
