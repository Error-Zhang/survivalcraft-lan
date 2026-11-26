using System.Linq;
using Game;
using SCIENEW.VoltNet;
using TemplatesDatabase;

namespace SCIENEW;

public class SubsystemIEInterfaceBlockBehavior : SubsystemBlockBehavior
{
	public const string BehaviorTag = "IEInterfaceBlockBehavior";

	private SubsystemFluidTransfer m_subsystemFluidTransfer;

	private SubsystemVoltNet m_subsystemVoltNet;

	public override int[] HandledBlocks => (from block in BlocksManager.Blocks
		where block != null && BlockInterfaceResolver.HasBehaviorTag(block.Behaviors, "IEInterfaceBlockBehavior")
		select block.BlockIndex).ToArray();

	public override void Load(ValuesDictionary valuesDictionary)
	{
		base.Load(valuesDictionary);
		m_subsystemFluidTransfer = Project.FindSubsystem<SubsystemFluidTransfer>(throwOnError: true);
		m_subsystemVoltNet = Project.FindSubsystem<SubsystemVoltNet>(throwOnError: true);
	}

	public override void OnBlockAdded(int value, int oldValue, int x, int y, int z)
	{
		IEInterfaceRoutingDecision routingDecision = GetRoutingDecision(value);
		if (routingDecision.HandlesFluidDevice)
		{
			m_subsystemFluidTransfer.OnDeviceAdded(x, y, z);
		}
		if (routingDecision.HandlesFluidPipe)
		{
			m_subsystemFluidTransfer.OnPipeAdded(x, y, z);
		}
		if (routingDecision.HandlesVoltDevice)
		{
			m_subsystemVoltNet.OnBlockAdded(value, oldValue, x, y, z);
		}
		if (routingDecision.HandlesVoltWire)
		{
			m_subsystemVoltNet.OnWireAdded(value, oldValue, x, y, z);
		}
	}

	public override void OnBlockRemoved(int value, int newValue, int x, int y, int z)
	{
		IEInterfaceRoutingDecision routingDecision = GetRoutingDecision(value);
		if (routingDecision.HandlesFluidDevice)
		{
			m_subsystemFluidTransfer.OnDeviceRemoved(x, y, z);
		}
		if (routingDecision.HandlesFluidPipe)
		{
			m_subsystemFluidTransfer.OnPipeRemoved(x, y, z);
		}
		if (routingDecision.HandlesVoltDevice)
		{
			m_subsystemVoltNet.OnBlockRemoved(value, newValue, x, y, z);
		}
		if (routingDecision.HandlesVoltWire)
		{
			m_subsystemVoltNet.OnWireRemoved(value, newValue, x, y, z);
		}
	}

	public override void OnBlockGenerated(int value, int x, int y, int z, bool isLoaded)
	{
		IEInterfaceRoutingDecision routingDecision = GetRoutingDecision(value);
		if (routingDecision.HandlesFluidDevice)
		{
			m_subsystemFluidTransfer.OnDeviceGenerated(x, y, z);
		}
		if (routingDecision.HandlesFluidPipe)
		{
			m_subsystemFluidTransfer.OnPipeGenerated(x, y, z);
		}
		if (routingDecision.HandlesVoltDevice)
		{
			m_subsystemVoltNet.OnBlockGenerated(value, x, y, z, isLoaded);
		}
		if (routingDecision.HandlesVoltWire)
		{
			m_subsystemVoltNet.OnWireGenerated(value, x, y, z, isLoaded);
		}
	}

	public override void OnBlockModified(int value, int oldValue, int x, int y, int z)
	{
		OnBlockRemoved(oldValue, value, x, y, z);
		OnBlockAdded(value, oldValue, x, y, z);
	}

	private static IEInterfaceRoutingDecision GetRoutingDecision(int value)
	{
		return IEInterfaceRoutingDecision.FromProvider(BlockInterfaceResolver.GetInterfaceProviderByValue(value));
	}
}
