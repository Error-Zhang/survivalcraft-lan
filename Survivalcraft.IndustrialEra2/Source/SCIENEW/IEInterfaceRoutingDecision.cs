using SCIENEW.VoltNet;

namespace SCIENEW;

public readonly record struct IEInterfaceRoutingDecision(bool HandlesFluidDevice, bool HandlesFluidPipe, bool HandlesVoltDevice, bool HandlesVoltWire)
{
	public static IEInterfaceRoutingDecision FromCapabilities(bool handlesFluidDevice, bool handlesFluidPipe, bool handlesVoltDevice, bool handlesVoltWire)
	{
		return new IEInterfaceRoutingDecision(handlesFluidDevice, handlesFluidPipe, handlesVoltDevice, handlesVoltWire);
	}

	public static IEInterfaceRoutingDecision FromProvider(object provider)
	{
		return FromCapabilities(provider is IFluidTransferrerDevice, provider is IFluidTransferrerPipeDevice, provider is IVoltDevice, provider is IVoltWireDevice);
	}
}
