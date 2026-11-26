using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class ReactorTurbineDevice : DefaultDevice, IEntityDevice<ComponentReactorTurbine>, IVoltDevice
{
	public ReactorTurbineDevice()
		: base("ReactorTurbineDevice")
	{
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 1;
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return face switch
		{
			4 => 142,
			5 => 98,
			_ => 128,
		};
	}

	public string GetEntityName(int value)
	{
		return "ReactorTurbine";
	}

	public Widget GetWidget(IInventory inventory, ComponentReactorTurbine component)
	{
		return new ReactorTurbineWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new ReactorTurbineDeviceBehavior(project),
			new VoltDeviceBehavior(project)
		};
	}

	public HashSet<int> GetFaceMask(int value)
	{
		return new HashSet<int> { 0, 1, 2, 3, 5 };
	}

	public VoltElement? GetVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
	{
		return new ReactorTurbineVoltElement(subsystemVoltNet, subsystemTerrain, position, blockValue);
	}

	public float GetStandardPL(int value)
	{
		return 0f;
	}

	public float GetStandardUL(int value)
	{
		return 4f;
	}
}
