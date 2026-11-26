using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class ElectricPreheaterDevice : DefaultDevice, IEntityDevice<ComponentElectricPreheater>, IVoltDevice
{
	public ElectricPreheaterDevice()
		: base("ElectricPreheaterDevice")
	{
	}

	public override int GetEmittedLightAmount(int value)
	{
		if (!IsLight(value))
		{
			return 0;
		}
		return 15;
	}

	public override float GetHeat(int value)
	{
		if (!IsLight(value))
		{
			return 0f;
		}
		return 0.66f;
	}

	protected static bool IsLight(int value)
	{
		return DevicesUtils.GetInnerData(value) >= 1;
	}

	public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
	{
		return IsLight(value);
	}

	public override bool IsTransparent_(int value)
	{
		return IsLight(value);
	}

	public override int GetShadowStrength(int value)
	{
		if (!IsLight(value))
		{
			return base.GetShadowStrength(value);
		}
		return -99;
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		if (!IsLight(value))
		{
			return 128;
		}
		return 160;
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 1;
	}

	public HashSet<int> GetFaceMask(int value)
	{
		return new HashSet<int> { 0, 1, 2, 3, 4, 5 };
	}

	public VoltElement? GetVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
	{
		return new ElectricPreheaterVoltElement(subsystemVoltNet, subsystemTerrain, position, blockValue);
	}

	public float GetStandardPL(int value)
	{
		return 1f;
	}

	public float GetStandardUL(int value)
	{
		return 1f;
	}

	public string GetEntityName(int value)
	{
		return "ElectricPreheater";
	}

	public Widget GetWidget(IInventory inventory, ComponentElectricPreheater component)
	{
		return null;
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new ElectricPreheaterDeviceBehavior(project),
			new VoltDeviceBehavior(project)
		};
	}
}
