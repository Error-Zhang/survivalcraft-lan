using Game;
using GameEntitySystem;

namespace SCIENEW;

public class GReactorCoreDevice : DefaultDevice, IEntityDevice<ComponentGReactorCore>
{
	public GReactorCoreDevice()
		: base("GReactorCoreDevice")
	{
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return face switch
		{
			5 => 141,
			4 => 138,
			_ => 137,
		};
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
		return 1f;
	}

	protected static bool IsLight(int value)
	{
		return DevicesUtils.GetInnerData(value) == 1;
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

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 2;
	}

	public string GetEntityName(int value)
	{
		return "GReactorCore";
	}

	public Widget GetWidget(IInventory inventory, ComponentGReactorCore component)
	{
		return new GReactorCoreWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new GReactorCoreDeviceBehavior(project)
		};
	}
}
