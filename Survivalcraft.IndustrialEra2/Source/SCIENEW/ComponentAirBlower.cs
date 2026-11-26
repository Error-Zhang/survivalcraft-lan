using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions.GameLogic;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentAirBlower : Component, IUpdateable, IAirBlowerComponent
{
	public SubsystemEnginePower m_subsystemEnginePower;

	public SubsystemAirBlowers m_subsystemAirBlowers;

	public ComponentBlockEntity m_componentBlockEntity;

	public SubsystemTerrain m_subsystemTerrain;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public AirBlowerPressureClass PressureClass => AirBlowerPressureClass.Blast;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_subsystemEnginePower = Project.FindSubsystem<SubsystemEnginePower>(throwOnError: true);
		m_subsystemAirBlowers = Project.FindSubsystem<SubsystemAirBlowers>(throwOnError: true);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
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
	}

	public bool IsWorking()
	{
		IEngineComponent engineComponent;
		return m_subsystemEnginePower.IsPowered(m_componentBlockEntity.Coordinates, out engineComponent);
	}

	public void Update(float dt)
	{
		ReplaceFurnace(m_componentBlockEntity.Coordinates);
	}

	public virtual void ReplaceFurnace(Point3 coordinates)
	{
		if (m_subsystemTerrain.Terrain.GetChunkAtCell(coordinates.X, coordinates.Z).IsValid())
		{
			int direction_Static = IlluminableDevice.GetDirection_Static(m_subsystemTerrain.Terrain.GetCellValue(coordinates.X, coordinates.Y, coordinates.Z));
			int deviceValueByType = DevicesBlockManager.GetDeviceValueByType<AirBlowerDevice>(IsWorking() ? (4 | direction_Static) : direction_Static);
			m_subsystemTerrain.ChangeCell(coordinates.X, coordinates.Y, coordinates.Z, deviceValueByType);
		}
	}
}
