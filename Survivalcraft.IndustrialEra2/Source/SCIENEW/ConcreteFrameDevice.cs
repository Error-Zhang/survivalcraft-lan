using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class ConcreteFrameDevice : ModelDevice
{
	public ConcreteFrameDevice()
		: base("ConcreteFrameDevice", "Models/ConcreteFrame", "ConcreteFrame", IndustrialModLoader.IETexture, Color.White)
	{
	}

	public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
	{
		return true;
	}

	public override bool IsTransparent_(int value)
	{
		return true;
	}

	public override string GetSoundMaterialName(SubsystemTerrain subsystemTerrain, int value)
	{
		return "Wood";
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return 186;
	}

	public override int GetShadowStrength(int value)
	{
		return 0;
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) - 1;
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new ConcreteFrameDevicePlacingBehavior(project),
			new ConcreteFrameDeviceFillingBehavior(project)
		};
	}

	public override int GetMaxStacking(int value)
	{
		return 40;
	}
}
