using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class GeneratorDevice : SixDirectionModelDevice, IVoltDevice
{
	public GeneratorDevice()
		: base("GeneratorDevice", "Models/Generator", "Generator", faceTransparent: true, useStandaloneMesh: false)
	{
	}

	public HashSet<int> GetFaceMask(int value)
	{
		return new HashSet<int> { GetDirection(value) };
	}

	public VoltElement? GetVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
	{
		return new GeneratorVoltElement(subsystemVoltNet, subsystemTerrain, position, blockValue);
	}

	public float GetStandardPL(int value)
	{
		return 1f;
	}

	public float GetStandardUL(int value)
	{
		return 1f;
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
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
}
