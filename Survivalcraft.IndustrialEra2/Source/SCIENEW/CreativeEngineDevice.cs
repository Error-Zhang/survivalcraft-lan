using Game;
using GameEntitySystem;

namespace SCIENEW;

public class CreativeEngineDevice : DefaultDevice, IEntityDevice<ComponentCreativeEngine>
{
	public CreativeEngineDevice()
		: base("CreativeEngineDevice")
	{
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) - 3;
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return 85;
	}

	public string GetEntityName(int value)
	{
		return "CreativeEngine";
	}

	public Widget GetWidget(IInventory inventory, ComponentCreativeEngine component)
	{
		return new CreativeEngineWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new CreativeEngineDeviceBehavior(project)
		};
	}
}
