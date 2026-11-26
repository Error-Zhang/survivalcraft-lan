using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class ElectricBlastFurnaceDevice : IlluminableDevice, IEntityDevice<ComponentElectricBlastFurnace_Alloy>, IVoltDevice
{
	public ElectricBlastFurnaceDevice()
		: base("ElectricBlastFurnaceDevice", 157, 158, 128, 128, 128, 128)
	{
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 1;
	}

	public string GetEntityName(int value)
	{
		return "ElectricBlastFurnace";
	}

	public Widget GetWidget(IInventory inventory, ComponentElectricBlastFurnace_Alloy component)
	{
		return new ElectricBlastFurnaceWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new ElectricBlastFurnaceDeviceBehavior(project),
			new VoltDeviceBehavior(project)
		};
	}

	public HashSet<int> GetFaceMask(int value)
	{
		return new HashSet<int> { 0, 1, 2, 3, 4, 5 };
	}

	public VoltElement? GetVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
	{
		return new ElectricBlastFurnaceVoltElement(subsystemVoltNet, subsystemTerrain, position, blockValue);
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
