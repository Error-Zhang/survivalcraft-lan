namespace SCIENEW.Events;

public readonly struct IndustrialModInitializedEvent(int deviceCount)
{
	public int DeviceCount { get; } = deviceCount;
}
