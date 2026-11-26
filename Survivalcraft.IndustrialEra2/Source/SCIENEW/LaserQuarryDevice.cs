using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class LaserQuarryDevice : FourDirectionalDevice, IEntityDevice<ComponentLaserQuarry>, IVoltDevice
{
	public enum Mode
	{
		Drill,
		Dispose
	}

	public LaserQuarryDevice()
		: base("LaserQuarryDevice", 116, 116, 162, 99, 99)
	{
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 1;
	}

	public string GetEntityName(int value)
	{
		return "LaserQuarry";
	}

	public Widget GetWidget(IInventory inventory, ComponentLaserQuarry component)
	{
		return new LaserQuarryWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new LaserQuarryDeviceBehavior(project),
			new VoltDeviceBehavior(project)
		};
	}

	public HashSet<int> GetFaceMask(int value)
	{
		return new HashSet<int> { 0, 1, 2, 3, 4, 5 };
	}

	public VoltElement? GetVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
	{
		return new LaserQuarryVoltElement(subsystemVoltNet, subsystemTerrain, position, blockValue);
	}

	public float GetStandardPL(int value)
	{
		return 12f;
	}

	public float GetStandardUL(int value)
	{
		return 1f;
	}
}
