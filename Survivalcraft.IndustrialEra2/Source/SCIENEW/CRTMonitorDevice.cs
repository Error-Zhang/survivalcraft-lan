using System;
using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class CRTMonitorDevice : FourDirectionModelDevice, IEntityDevice<ComponentCRTMonitor>, IVoltDevice
{
	public CRTMonitorDevice()
		: base("CRTMonitorDevice", "Models/CRTMonitor", "CRTMonitor")
	{
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 1;
	}

	public string GetEntityName(int value)
	{
		return "CRTMonitor";
	}

	public Widget GetWidget(IInventory inventory, ComponentCRTMonitor component)
	{
		throw new NotImplementedException();
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new CRTMonitorDeviceBehavior(project),
			new VoltDeviceBehavior(project)
		};
	}

	public HashSet<int> GetFaceMask(int value)
	{
		HashSet<int> hashSet = new HashSet<int>();
		hashSet.Add(0);
		hashSet.Add(1);
		hashSet.Add(2);
		hashSet.Add(3);
		hashSet.Add(4);
		hashSet.Add(5);
		hashSet.Remove(GetDirection(value));
		return hashSet;
	}

	public VoltElement? GetVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
	{
		return new CRTMonitorVoltElement(subsystemVoltNet, subsystemTerrain, position, blockValue);
	}

	public float GetStandardPL(int value)
	{
		return 2f;
	}

	public float GetStandardUL(int value)
	{
		return 1f;
	}
}
