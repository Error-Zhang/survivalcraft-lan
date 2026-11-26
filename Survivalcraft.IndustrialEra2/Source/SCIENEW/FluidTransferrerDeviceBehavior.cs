using GameEntitySystem;

namespace SCIENEW;

public class FluidTransferrerDeviceBehavior : DeviceBehavior
{
	public SubsystemFluidTransfer m_subsystemFluidTransfer;

	public FluidTransferrerDeviceBehavior(Project project)
		: base(project)
	{
		m_subsystemFluidTransfer = project.FindSubsystem<SubsystemFluidTransfer>(throwOnError: true);
	}

	public override void OnBlockGenerated(int value, int x, int y, int z, bool isLoaded)
	{
		m_subsystemFluidTransfer.OnDeviceGenerated(x, y, z);
	}

	public override void OnBlockAdded(int value, int oldValue, int x, int y, int z)
	{
		m_subsystemFluidTransfer.OnDeviceAdded(x, y, z);
	}

	public override void OnBlockRemoved(int value, int newValue, int x, int y, int z)
	{
		m_subsystemFluidTransfer.OnDeviceRemoved(x, y, z);
	}

	public override void OnBlockModified(int value, int oldValue, int x, int y, int z)
	{
		m_subsystemFluidTransfer.OnDeviceModified(x, y, z);
	}
}
