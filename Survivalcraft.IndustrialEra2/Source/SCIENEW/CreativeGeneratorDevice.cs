using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class CreativeGeneratorDevice : DefaultDevice, IEntityDevice<ComponentCreativeGenerator>, IVoltDevice
{
	public CreativeGeneratorDevice()
		: base("CreativeGeneratorDevice")
	{
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + -3;
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return 84;
	}

	public string GetEntityName(int value)
	{
		return "CreativeGenerator";
	}

	public Widget GetWidget(IInventory inventory, ComponentCreativeGenerator component)
	{
		return new CreativeGeneratorWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new CreativeGeneratorDeviceBehavior(project),
			new VoltDeviceBehavior(project)
		};
	}

	public HashSet<int> GetFaceMask(int value)
	{
		return new HashSet<int> { 0, 1, 2, 3, 4, 5 };
	}

	public VoltElement? GetVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
	{
		return new CreativeGeneratorVoltElement(subsystemVoltNet, subsystemTerrain, position, blockValue);
	}

	public float GetStandardPL(int value)
	{
		return 0f;
	}

	public float GetStandardUL(int value)
	{
		return 0f;
	}
}
