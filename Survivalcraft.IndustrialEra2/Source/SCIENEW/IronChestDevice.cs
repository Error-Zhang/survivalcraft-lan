using Game;
using GameEntitySystem;

namespace SCIENEW;

public class IronChestDevice : FourDirectionalDevice, IEntityDevice<ComponentChest>
{
	public IronChestDevice()
		: base("IronChestDevice", 134, 134, 134)
	{
	}

	public string GetEntityName(int value)
	{
		return "IronChest";
	}

	public Widget GetWidget(IInventory inventory, ComponentChest component)
	{
		return new IronChestWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new IronChestDeviceBehavior(project)
		};
	}
}
