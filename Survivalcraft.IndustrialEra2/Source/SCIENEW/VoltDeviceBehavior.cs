using GameEntitySystem;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class VoltDeviceBehavior : DeviceBehavior
{
	public SubsystemVoltNet m_subsystemVoltNet;

	public VoltDeviceBehavior(Project project)
		: base(project)
	{
		m_subsystemVoltNet = project.FindSubsystem<SubsystemVoltNet>(throwOnError: true);
	}

	public override void OnBlockAdded(int value, int oldValue, int x, int y, int z)
	{
		m_subsystemVoltNet.OnBlockAdded(value, oldValue, x, y, z);
	}

	public override void OnBlockRemoved(int value, int newValue, int x, int y, int z)
	{
		m_subsystemVoltNet.OnBlockRemoved(value, newValue, x, y, z);
	}

	public override void OnBlockGenerated(int value, int x, int y, int z, bool isLoaded)
	{
		m_subsystemVoltNet.OnBlockGenerated(value, x, y, z, isLoaded);
	}

	public override void OnBlockModified(int value, int oldValue, int x, int y, int z)
	{
		m_subsystemVoltNet.OnBlockModified(value, oldValue, x, y, z);
	}
}
