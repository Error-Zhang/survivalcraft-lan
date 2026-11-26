using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class WindTurbineDevice : FourDirectionModelDevice, IEntityDevice<ComponentWindTurbine>, IVoltDevice
{
	public WindTurbineDevice()
		: base("WindTurbineDevice", "Models/WindTurbine", "WindTurbine")
	{
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 1;
	}

	public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value)
	{
		return Block.m_defaultCollisionBoxes;
	}

	public string GetEntityName(int value)
	{
		return "WindTurbine";
	}

	public Widget GetWidget(IInventory inventory, ComponentWindTurbine component)
	{
		return new WindTurbineWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new WindTurbineDeviceBehavior(project),
			new VoltDeviceBehavior(project)
		};
	}

	public HashSet<int> GetFaceMask(int value)
	{
		return new HashSet<int> { 5 };
	}

	public VoltElement? GetVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
	{
		return new WindTurbineVoltElement(subsystemVoltNet, subsystemTerrain, position, blockValue);
	}

	public float GetStandardPL(int value)
	{
		return 0f;
	}

	public float GetStandardUL(int value)
	{
		return 1f;
	}
}
