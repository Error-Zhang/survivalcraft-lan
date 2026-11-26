using System.Collections.Generic;

namespace SCIENEW.Events;

public sealed class DeviceCatalogBuildingEvent
{
	public List<DeviceBlock> Devices { get; }

	public DeviceCatalogBuildingEvent(List<DeviceBlock> devices)
	{
		Devices = devices;
	}
}
