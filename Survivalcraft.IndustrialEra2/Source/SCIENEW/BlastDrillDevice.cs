using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class BlastDrillDevice : FourDirectionalDevice, IEntityDevice<ComponentBlastDrill>, IVoltDevice
{
	public enum Mode
	{
		Drill,
		Dispose
	}

	public BlastDrillDevice()
		: base("BlastDrillDevice", 116, 116, 163, 99, 99)
	{
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 1;
	}

	public string GetEntityName(int value)
	{
		return "BlastDrill";
	}

	public Widget GetWidget(IInventory inventory, ComponentBlastDrill component)
	{
		return new BlastDrillWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new BlastDrillDeviceBehavior(project),
			new VoltDeviceBehavior(project)
		};
	}

	public HashSet<int> GetFaceMask(int value)
	{
		return new HashSet<int> { 0, 1, 2, 3, 4, 5 };
	}

	public VoltElement? GetVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
	{
		return new BlastDrillVoltElement(subsystemVoltNet, subsystemTerrain, position, blockValue);
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
