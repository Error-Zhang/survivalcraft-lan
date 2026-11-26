using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Utils;

namespace SCIENEW;

public class FluidPipeDeviceBehavior : DeviceBehavior
{
	public SubsystemFluidTransfer m_subsystemFluidTransfer;

	public FluidPipeDeviceBehavior(Project project)
		: base(project)
	{
		m_subsystemFluidTransfer = project.FindSubsystem<SubsystemFluidTransfer>(throwOnError: true);
	}

	public override void OnBlockGenerated(int value, int x, int y, int z, bool isLoaded)
	{
		m_subsystemFluidTransfer.OnPipeGenerated(x, y, z);
	}

	public override void OnBlockAdded(int value, int oldValue, int x, int y, int z)
	{
		m_subsystemFluidTransfer.OnPipeAdded(x, y, z);
	}

	public override void OnBlockRemoved(int value, int newValue, int x, int y, int z)
	{
		m_subsystemFluidTransfer.OnPipeRemoved(x, y, z);
	}

	public override void OnBlockModified(int value, int oldValue, int x, int y, int z)
	{
		DeviceBlock device = DevicesBlockManager.GetDevice(oldValue);
		DeviceBlock device2 = DevicesBlockManager.GetDevice(value);
		if (device.GetCraftingId() != device2.GetCraftingId())
		{
			BlockEntityUtils.RemoveBlockEntity(Project.FindSubsystem<SubsystemTerrain>(throwOnError: true), new Point3(x, y, z));
			m_subsystemFluidTransfer.OnPipeAdded(x, y, z);
		}
		else
		{
			m_subsystemFluidTransfer.OnPipeModified(x, y, z);
		}
	}
}
