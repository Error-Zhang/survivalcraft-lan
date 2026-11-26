using Game;
using GameEntitySystem;

namespace SCIENEW;

public class SteelChestDevice : FourDirectionalDevice, IEntityDevice<ComponentChest>
{
	public SteelChestDevice()
		: base("SteelChestDevice", 135, 135, 135)
	{
	}

	public string GetEntityName(int value)
	{
		return "SteelChest";
	}

	public Widget GetWidget(IInventory inventory, ComponentChest component)
	{
		return new SteelChestWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new SteelChestDeviceBehavior(project)
		};
	}
}
