using System;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentMirror : Component, ILaserHandler, IFocus, IUpdateable
{
	public const int MaxPitch = 90;

	public const int MaxYaw = 180;

	public const int MinPitch = -90;

	public const int MinYaw = -180;

	public Vector3 m_normal;

	public ComponentBlockEntity m_componentBlockEntity;

	public SubsystemLaser m_subsystemLaser;

	public SubsystemTime m_subsystemTime;

	public ComponentIEModel m_componentIEModel;

	private Vector3 m_previousNormal = Vector3.Zero;

	private Matrix m_lookAtMatrix = Matrix.Identity;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_componentIEModel = Entity.FindComponent<ComponentIEModel>(throwOnError: true);
		m_subsystemLaser = Project.FindSubsystem<SubsystemLaser>(throwOnError: true);
		m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
		m_normal = valuesDictionary.GetValue("Normal", new Vector3(0f, 1f, 0f));
		MirrorDevice device = DevicesBlockManager.GetDevice<MirrorDevice>();
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
		valuesDictionary.SetValue("Normal", m_normal);
	}

	public void Focus(Vector3 newPosition)
	{
		Vector3 vector = new Vector3(m_componentBlockEntity.Coordinates) + new Vector3(0.5f);
		m_normal = (newPosition - vector).Normalize();
	}

	public Vector3 GetFocus()
	{
		return m_normal;
	}

	public void SetFocus(Vector3 focus)
	{
		m_normal = focus;
	}

	public bool OnLaserHit(Vector3 renderStart, Vector3 incidentDirection, TerrainRaycastResult hit, Color color, double energy)
	{
		m_subsystemLaser.RenderLaser(renderStart, new Vector3(m_componentBlockEntity.Coordinates) + new Vector3(0.5f) + m_normal * 0.0625f, color, energy);
		if (incidentDirection.Dot(m_normal) <= 0f)
		{
			Vector3 vector = incidentDirection.Reflect(m_normal);
			m_subsystemLaser.Spawn(new Vector3(m_componentBlockEntity.Coordinates) + new Vector3(0.5f) + vector * 0.9f, new Vector3(m_componentBlockEntity.Coordinates) + new Vector3(0.5f) + m_normal * 0.0625f, vector, color, energy);
		}
		return true;
	}

	public void Update(float dt)
	{
		Matrix value = Matrix.CreateTranslation(new Vector3(m_componentBlockEntity.Coordinates) + new Vector3(0.5f, 0f, 0.5f));
		if (m_previousNormal != m_normal)
		{
			m_previousNormal = m_normal;
			m_lookAtMatrix = Matrix.CreateRotationX(-(float)Math.PI / 2f) * Matrix.Invert(Matrix.CreateLookAt(Vector3.Zero, m_normal, Vector3.UnitY));
		}
		m_componentIEModel.SetBoneTransform(m_componentIEModel.Model.RootBone.Index, value);
		m_componentIEModel.SetBoneTransform("Mirror", m_lookAtMatrix);
	}

	public (int Yaw, int Pitch) GetYawPitch()
	{
		int item = (int)MathUtils.RadToDeg(Math.Atan2(m_normal.X, m_normal.Z));
		int item2 = (int)MathUtils.RadToDeg(Math.Asin(m_normal.Y));
		return (Yaw: item, Pitch: item2);
	}

	public void SetYawPitch(float yaw, float pitch)
	{
		pitch = MathUtils.DegToRad(pitch);
		yaw = MathUtils.DegToRad(yaw);
		m_normal = new Vector3(MathF.Sin(yaw) * MathF.Cos(pitch), MathF.Sin(pitch), MathF.Cos(yaw) * MathF.Cos(pitch)).Normalize();
	}
}
