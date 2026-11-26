using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions.GameLogic;
using SCIENEW.VoltNet;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentElectricAirBlower : Component, IUpdateable, IAirBlowerComponent
{
	public ComponentBlockEntity m_componentBlockEntity;

	public ComponentIEModel m_componentIEModel;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemVoltNet m_subsystemVoltNet;

	public SubsystemParticles m_subsystemParticles;

	public SubsystemTime m_subsystemTime;

	public SubsystemUpdrafts m_subsystemUpdrafs;

	public SubsystemAirBlowers m_subsystemAirBlowers;

	public Game.Random m_random = new Game.Random();

	public WorkState m_workState;

	public CellFace m_cellFace;

	public bool m_added;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public AirBlowerPressureClass PressureClass => AirBlowerPressureClass.Turbo;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemVoltNet = Project.FindSubsystem<SubsystemVoltNet>(throwOnError: true);
		m_componentIEModel = Entity.FindComponent<ComponentIEModel>(throwOnError: true);
		m_subsystemParticles = Project.FindSubsystem<SubsystemParticles>(throwOnError: true);
		m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
		m_subsystemUpdrafs = Project.FindSubsystem<SubsystemUpdrafts>(throwOnError: true);
		m_subsystemAirBlowers = Project.FindSubsystem<SubsystemAirBlowers>(throwOnError: true);
		m_cellFace = new CellFace
		{
			Point = m_componentBlockEntity.Coordinates,
			Face = 4
		};
		ElectricAirBlowerDevice device = DevicesBlockManager.GetDevice<ElectricAirBlowerDevice>();
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
		m_subsystemAirBlowers.Add(m_componentBlockEntity.Coordinates, this);
	}

	public override void OnEntityRemoved()
	{
		base.OnEntityRemoved();
		m_subsystemAirBlowers.Remove(m_componentBlockEntity.Coordinates);
		m_subsystemUpdrafs.RemoveUpdraft(m_cellFace);
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
		if (m_workState == WorkState.Active && m_subsystemTime.PeriodicGameTimeEvent(m_random.Float(0.1f, 0.3f), (double)GetHashCode() % 1000.0 / 1000.0))
		{
			m_subsystemParticles.AddParticleSystem(new AirFlowParticleSystem(m_subsystemTerrain, m_componentBlockEntity.Coordinates, m_random.Int(1, 3), isFadeIn: false));
		}
		if (m_workState == WorkState.Active && !m_added)
		{
			m_added = true;
			m_subsystemUpdrafs.AddUpdraft(m_cellFace);
		}
		if (m_workState != WorkState.Active && m_added)
		{
			m_added = false;
			m_subsystemUpdrafs.RemoveUpdraft(m_cellFace);
		}
		ReplaceFurnace(m_componentBlockEntity.Coordinates);
		UpdateDrawing();
	}

	public virtual void ReplaceFurnace(Point3 coordinates)
	{
		if (m_subsystemTerrain.Terrain.GetChunkAtCell(coordinates.X, coordinates.Z).IsValid())
		{
			int deviceValueByType = DevicesBlockManager.GetDeviceValueByType<ElectricAirBlowerDevice>((m_workState == WorkState.Active) ? 1 : 0);
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

	public bool IsWorking()
	{
		return m_workState == WorkState.Active;
	}
}
