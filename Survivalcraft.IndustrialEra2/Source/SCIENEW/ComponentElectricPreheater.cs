using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions.GameLogic;
using SCIENEW.VoltNet;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentElectricPreheater : Component, IUpdateable, IFireBoxComponent
{
	public ComponentBlockEntity m_componentBlockEntity;

	public SubsystemVoltNet m_subsystemVoltNet;

	public SubsystemTerrain m_subsystemTerrain;

	public WorkState m_workState;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public override void OnEntityAdded()
	{
		base.OnEntityAdded();
		Project.FindSubsystem<SubsystemFireBoxes>(throwOnError: true).Add(m_componentBlockEntity.Coordinates, this);
	}

	public override void OnEntityRemoved()
	{
		base.OnEntityRemoved();
		Project.FindSubsystem<SubsystemFireBoxes>(throwOnError: true).Remove(m_componentBlockEntity.Coordinates);
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemVoltNet = Project.FindSubsystem<SubsystemVoltNet>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
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
	}

	public virtual void ReplaceFurnace(Point3 coordinates)
	{
		if (m_subsystemTerrain.Terrain.GetChunkAtCell(coordinates.X, coordinates.Z).IsValid())
		{
			int deviceValueByType = DevicesBlockManager.GetDeviceValueByType<ElectricPreheaterDevice>(IsPreheating() ? 1 : 0);
			m_subsystemTerrain.ChangeCell(coordinates.X, coordinates.Y, coordinates.Z, deviceValueByType);
		}
	}

	public bool IsPreheating()
	{
		return m_workState == WorkState.Active;
	}
}
