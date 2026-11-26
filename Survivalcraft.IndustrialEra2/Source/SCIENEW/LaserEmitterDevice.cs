using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class LaserEmitterDevice : SixDirectionModelDevice, IEntityDevice<ComponentLaserEmitter>, IVoltDevice
{
	public LaserEmitterDevice()
		: base("LaserEmitterDevice", "Models/LaserEmitter", "LaserEmitter", faceTransparent: true, useStandaloneMesh: false)
	{
	}

	public HashSet<int> GetFaceMask(int value)
	{
		return new HashSet<int> { CellFace.OppositeFace(GetDirection(value)) };
	}

	public VoltElement? GetVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
	{
		return new LaserEmitterVoltElement(subsystemVoltNet, subsystemTerrain, position, blockValue);
	}

	public float GetStandardPL(int value)
	{
		return 24f;
	}

	public float GetStandardUL(int value)
	{
		return 1f;
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new LaserEmitterDeviceBehavior(project),
			new VoltDeviceBehavior(project)
		};
	}

	public override bool IsTransparent_(int value)
	{
		return true;
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 1;
	}

	public string GetEntityName(int value)
	{
		return "LaserEmitter";
	}

	public Widget GetWidget(IInventory inventory, ComponentLaserEmitter component)
	{
		return null;
	}
}
