using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Utils;

namespace SCIENEW.VoltNet;

public class VoltWireDeviceBehavior : DeviceBehavior
{
	public SubsystemVoltNet m_subsystemVoltNet;

	public VoltWireDeviceBehavior(Project project)
		: base(project)
	{
		m_subsystemVoltNet = project.FindSubsystem<SubsystemVoltNet>(throwOnError: true);
	}

	public override void OnBlockAdded(int value, int oldValue, int x, int y, int z)
	{
		m_subsystemVoltNet.OnWireAdded(value, oldValue, x, y, z);
	}

	public override void OnBlockRemoved(int value, int newValue, int x, int y, int z)
	{
		m_subsystemVoltNet.OnWireRemoved(value, newValue, x, y, z);
	}

	public override void OnBlockGenerated(int value, int x, int y, int z, bool isLoaded)
	{
		m_subsystemVoltNet.OnWireGenerated(value, x, y, z, isLoaded);
	}

	public override void OnBlockModified(int value, int oldValue, int x, int y, int z)
	{
		DeviceBlock device = DevicesBlockManager.GetDevice(oldValue);
		DeviceBlock device2 = DevicesBlockManager.GetDevice(value);
		if (device.GetCraftingId() != device2.GetCraftingId())
		{
			BlockEntityUtils.RemoveBlockEntity(Project.FindSubsystem<SubsystemTerrain>(throwOnError: true), new Point3(x, y, z));
			m_subsystemVoltNet.OnWireAdded(value, oldValue, x, y, z);
		}
		else
		{
			m_subsystemVoltNet.OnWireModified(value, oldValue, x, y, z);
		}
	}
}
