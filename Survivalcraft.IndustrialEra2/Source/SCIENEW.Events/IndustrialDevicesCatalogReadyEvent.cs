namespace SCIENEW.Events;

public readonly struct IndustrialDevicesCatalogReadyEvent(int deviceCount)
{
	public int DeviceCount { get; } = deviceCount;
}
