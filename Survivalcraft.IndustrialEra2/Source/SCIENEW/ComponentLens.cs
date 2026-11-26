using System;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentLens : Component, IFocus, ILaserHandler, IUpdateable
{
	public const int MaxPitch = 90;

	public const int MaxYaw = 180;

	public const int MinPitch = -90;

	public const int MinYaw = -180;

	public Vector3 m_normal;

	public Color? m_color;

	public double m_energy;

	public SubsystemLaser m_subsystemLaser;

	public ComponentBlockEntity m_componentBlockEntity;

	public ComponentIEModel m_componentIEModel;

	private Vector3 m_previousNormal = Vector3.Zero;

	private Matrix m_lookAtMatrix = Matrix.Identity;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_subsystemLaser = Project.FindSubsystem<SubsystemLaser>(throwOnError: true);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_componentIEModel = Entity.FindComponent<ComponentIEModel>(throwOnError: true);
		m_normal = valuesDictionary.GetValue("Normal", Vector3.UnitY);
		LensDevice device = DevicesBlockManager.GetDevice<LensDevice>();
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
		Vector3 vector = new Vector3(m_componentBlockEntity.Coordinates);
		m_normal = (newPosition - vector - new Vector3(0.5f)).Normalize();
	}

	public Vector3 GetFocus()
	{
		return m_normal;
	}

	public void SetFocus(Vector3 focus)
	{
		m_normal = focus;
	}

	public bool OnLaserHit(Vector3 renderStart, Vector3 incident, TerrainRaycastResult hit, Color color, double energy)
	{
		Vector3 vector = new Vector3(m_componentBlockEntity.Coordinates);
		m_subsystemLaser.RenderLaser(renderStart, vector + new Vector3(0.5f), color, energy);
		m_energy += energy / 1.2;
		m_color = ((!m_color.HasValue) ? new Color?(color) : ((m_color + color) / 2f));
		return true;
	}

	public void Update(float dt)
	{
		if (!IndustrialEraLanAdapter.IsRemote(Project) && m_energy > 0.0)
		{
			Vector3 vector = new Vector3(m_componentBlockEntity.Coordinates);
			m_subsystemLaser.Spawn(vector + new Vector3(0.5f) + m_normal * 0.9f, vector + new Vector3(0.5f), m_normal, m_color ?? Color.White, m_energy);
			m_color = null;
			m_energy = 0.0;
		}
		Matrix value = Matrix.CreateTranslation(new Vector3(m_componentBlockEntity.Coordinates) + new Vector3(0.5f, 0f, 0.5f));
		if (m_previousNormal != m_normal)
		{
			m_previousNormal = m_normal;
			m_lookAtMatrix = Matrix.CreateRotationX(-(float)Math.PI / 2f) * Matrix.CreateLookAt(Vector3.Zero, m_normal, Vector3.UnitY).Invert();
		}
		m_componentIEModel.SetBoneTransform(m_componentIEModel.Model.RootBone.Index, value);
		m_componentIEModel.SetBoneTransform("Lens", m_lookAtMatrix);
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
