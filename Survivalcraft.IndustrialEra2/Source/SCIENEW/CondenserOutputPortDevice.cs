using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class CondenserOutputPortDevice : SixDirectionalDevice, IVoltDevice
{
	public CondenserOutputPortDevice()
		: base("CondenserOutputPortDevice", 30, 147, 99)
	{
	}

	public HashSet<int> GetFaceMask(int value)
	{
		return new HashSet<int> { DevicesUtils.GetInnerData(value) };
	}

	public VoltElement? GetVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
	{
		return new CondenserPortVoltElement(subsystemVoltNet, subsystemTerrain, position, blockValue, CondenserPortRole.Output);
	}

	public float GetStandardPL(int value)
	{
		return 0f;
	}

	public float GetStandardUL(int value)
	{
		return 1f;
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 1;
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new CondenserPortDeviceBehavior(project),
			new VoltDeviceBehavior(project)
		};
	}
}
