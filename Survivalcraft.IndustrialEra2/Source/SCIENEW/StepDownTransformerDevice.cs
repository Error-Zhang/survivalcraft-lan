using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class StepDownTransformerDevice : SixDirectionModelDevice, IVoltDevice
{
	public StepDownTransformerDevice()
		: base("StepDownTransformerDevice", "Models/Transformer", "StepDownTransformer", faceTransparent: false, useStandaloneMesh: false)
	{
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 1;
	}

	public HashSet<int> GetFaceMask(int value)
	{
		return new HashSet<int>
		{
			GetDirection(value),
			CellFace.OppositeFace(GetDirection(value))
		};
	}

	public VoltElement? GetVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
	{
		return new TransformerVoltElement(subsystemVoltNet, subsystemTerrain, position, blockValue, 0.5f, 0.9f);
	}

	public float GetStandardPL(int value)
	{
		return -64f;
	}

	public float GetStandardUL(int value)
	{
		return -4f;
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new VoltDeviceBehavior(project)
		};
	}
}
