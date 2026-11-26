using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class AF_BoxGeneratorDevice : ModelDevice, IEntityDevice<ComponentAF_BoxGenerator>, IVoltDevice
{
	public AF_BoxGeneratorDevice()
		: base("AF_BoxGeneratorDevice", "Models/AF_BoxGenerator", "AF_BoxGenerator", ContentManager.Get<Texture2D>("Textures/AF_BoxGenerator"), Color.White)
	{
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 10086;
	}

	public string GetEntityName(int value)
	{
		return "AF_BoxGenerator";
	}

	public Widget GetWidget(IInventory inventory, ComponentAF_BoxGenerator component)
	{
		return new AF_BoxGeneratorWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new AF_BoxGeneratorDeviceBehavior(project),
			new VoltDeviceBehavior(project)
		};
	}

	public HashSet<int> GetFaceMask(int value)
	{
		return new HashSet<int> { 0, 1, 2, 3, 4, 5 };
	}

	public VoltElement? GetVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
	{
		return new AF_BoxGeneratorVoltElement(subsystemVoltNet, subsystemTerrain, position, blockValue);
	}

	public float GetStandardPL(int value)
	{
		return 0f;
	}

	public float GetStandardUL(int value)
	{
		return 0f;
	}

	public override bool IsDisplay()
	{
		return IndustrialModLoader.IsAprilFools;
	}
}
