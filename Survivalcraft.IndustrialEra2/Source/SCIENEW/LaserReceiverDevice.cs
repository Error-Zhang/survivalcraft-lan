using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class LaserReceiverDevice : SixDirectionModelDevice, IEntityDevice<ComponentLaserReceiver>, IVoltDevice
{
	public LaserReceiverDevice()
		: base("LaserReceiverDevice", "Models/LaserReceiver", "LaserReceiver", faceTransparent: true, useStandaloneMesh: false)
	{
	}

	public HashSet<int> GetFaceMask(int value)
	{
		return new HashSet<int> { CellFace.OppositeFace(GetDirection(value)) };
	}

	public VoltElement? GetVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
	{
		return new LaserReceiverVoltElement(subsystemVoltNet, subsystemTerrain, position, blockValue);
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
			new LaserReceiverDeviceBehavior(project),
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
		return "LaserReceiver";
	}

	public Widget GetWidget(IInventory inventory, ComponentLaserReceiver component)
	{
		return null;
	}
}
