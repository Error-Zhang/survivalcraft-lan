using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class GReactorTopDevice : ModelDevice, IEntityDevice<ComponentGReactorTop>
{
	public GReactorTopDevice()
		: base("GReactorTopDevice", "Models/GReactorTop", "GReactorTop", IndustrialModLoader.IETexture, Color.White)
	{
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 2;
	}

	public override bool IsTransparent_(int value)
	{
		return true;
	}

	public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
	{
		return true;
	}

	public override int GetShadowStrength(int value)
	{
		return 0;
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return 141;
	}

	public string GetEntityName(int value)
	{
		return "GReactorTop";
	}

	public Widget GetWidget(IInventory inventory, ComponentGReactorTop component)
	{
		return new GReactorTopWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new GReactorTopDeviceBehavior(project)
		};
	}
}
