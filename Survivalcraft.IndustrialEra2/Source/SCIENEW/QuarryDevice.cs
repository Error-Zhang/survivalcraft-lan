using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class QuarryDevice : FourDirectionalDevice, IEntityDevice<ComponentQuarry>, IVoltDevice
{
	public enum Mode
	{
		Drill,
		Dispose
	}

	public QuarryDevice()
		: base("QuarryDevice", 116, 116, 116, 99, 99)
	{
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 1;
	}

	public string GetEntityName(int value)
	{
		return "Quarry";
	}

	public Widget GetWidget(IInventory inventory, ComponentQuarry component)
	{
		return new QuarryWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new QuarryDeviceBehavior(project),
			new VoltDeviceBehavior(project)
		};
	}

	public HashSet<int> GetFaceMask(int value)
	{
		return new HashSet<int> { 0, 1, 2, 3, 4, 5 };
	}

	public VoltElement? GetVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
	{
		return new QuarryVoltElement(subsystemVoltNet, subsystemTerrain, position, blockValue);
	}

	public float GetStandardPL(int value)
	{
		return 6f;
	}

	public float GetStandardUL(int value)
	{
		return 1f;
	}
}
