using System.Collections.Generic;
using GameEntitySystem;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class WireCubeDevice : DefaultDevice, IVoltWireDevice
{
	public WireCubeDevice()
		: base("WireCubeDevice")
	{
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 1;
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		if ((uint)(face - 4) > 1u)
		{
			return 156;
		}
		return 99;
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new VoltWireDeviceBehavior(project)
		};
	}

	public HashSet<int> GetFaceMask(int value)
	{
		return new HashSet<int> { 0, 1, 2, 3, 4, 5 };
	}

	public float GetPL(int value)
	{
		return IEConstants.VoltNet.WireCube.PL;
	}

	public float GetUL(int value)
	{
		return IEConstants.VoltNet.WireCube.UL;
	}

	public Materials GetMaterial()
	{
		return Materials.Copper;
	}
}
