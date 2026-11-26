using Engine;
using GameEntitySystem;

namespace SCIENEW;

public class BeaconDeviceBehavior : DeviceBehavior
{
	public SubsystemBeacons m_subsystemBeacons;

	public BeaconDeviceBehavior(Project project)
		: base(project)
	{
		m_subsystemBeacons = Project.FindSubsystem<SubsystemBeacons>(throwOnError: true);
	}

	public override void OnBlockAdded(int value, int oldValue, int x, int y, int z)
	{
		m_subsystemBeacons.AddPoint(new Point3(x, y, z));
	}

	public override void OnBlockRemoved(int value, int newValue, int x, int y, int z)
	{
		m_subsystemBeacons.RemovePoint(new Point3(x, y, z));
	}

	public override void OnBlockGenerated(int value, int x, int y, int z, bool isLoaded)
	{
		m_subsystemBeacons.AddPoint(new Point3(x, y, z));
	}
}
