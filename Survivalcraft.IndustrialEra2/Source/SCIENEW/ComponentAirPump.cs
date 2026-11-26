using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions.GameLogic;
using SCIENEW.VoltNet;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentAirPump : Component, IUpdateable
{
	public ComponentBlockEntity m_componentBlockEntity;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemVoltNet m_subsystemVoltNet;

	public SubsystemAirPumps m_subsystemAirPumps;

	public ComponentIEModel m_componentIEModel;

	public WorkState m_workState;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemVoltNet = Project.FindSubsystem<SubsystemVoltNet>(throwOnError: true);
		m_subsystemAirPumps = Project.FindSubsystem<SubsystemAirPumps>(throwOnError: true);
		m_componentIEModel = Entity.FindComponent<ComponentIEModel>(throwOnError: true);
		AirPumpDevice device = DevicesBlockManager.GetDevice<AirPumpDevice>();
		if (device != null)
		{
			m_componentIEModel.SetModel(device.m_model);
			m_componentIEModel.SetTexture(device.m_texture);
			m_componentIEModel.MainBoneName = device.m_model.RootBone.Name;
			m_componentIEModel.RenderingMode = ModelRenderingMode.AlphaThreshold;
		}
	}

	public override void OnEntityAdded()
	{
		base.OnEntityAdded();
		m_subsystemAirPumps.Add(m_componentBlockEntity.Coordinates, this);
	}

	public override void OnEntityRemoved()
	{
		base.OnEntityRemoved();
		m_subsystemAirPumps.Remove(m_componentBlockEntity.Coordinates, this);
	}

	public void Update(float dt)
	{
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		VoltElement voltElement = m_subsystemVoltNet.GetVoltElement(coordinates);
		if (voltElement == null)
		{
			m_workState = WorkState.Broken;
			return;
		}
		m_workState = voltElement.CurrentWorkState;
		ReplaceFurnace(m_componentBlockEntity.Coordinates);
		UpdateDrawing();
	}

	public virtual void ReplaceFurnace(Point3 coordinates)
	{
		if (m_subsystemTerrain.Terrain.GetChunkAtCell(coordinates.X, coordinates.Z).IsValid())
		{
			int deviceValueByType = DevicesBlockManager.GetDeviceValueByType<AirPumpDevice>((m_workState == WorkState.Active) ? 1 : 0);
			m_subsystemTerrain.ChangeCell(coordinates.X, coordinates.Y, coordinates.Z, deviceValueByType);
		}
	}

	public void UpdateDrawing()
	{
		if (m_workState != WorkState.Active)
		{
			m_componentIEModel.SetBoneTransform("Fan", null);
			m_componentIEModel.SetBoneTransform(m_componentIEModel.Model.RootBone.Index, null);
			return;
		}
		Matrix value = Matrix.CreateTranslation(new Vector3(m_componentBlockEntity.Coordinates) + new Vector3(0.5f, 0f, 0.5f));
		Matrix value2 = Matrix.CreateRotationY((float)(Time.RealTime * 20.0 % 6.2831854820251465));
		m_componentIEModel.SetBoneTransform(m_componentIEModel.Model.RootBone.Index, value);
		m_componentIEModel.SetBoneTransform("Fan", value2);
	}

	public WorkState GetWorkState()
	{
		return m_workState;
	}
}
