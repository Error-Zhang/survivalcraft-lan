using Game;
using GameEntitySystem;

namespace SCIENEW;

public class OpenHearthFurnaceDevice : IlluminableDevice, IEntityDevice<ComponentHearthFurnace>
{
	public OpenHearthFurnaceDevice()
		: base("OpenHearthFurnaceDevice", 243, 229, 228, 228, 228)
	{
		DownSlot = 228;
	}

	public string GetEntityName(int value)
	{
		return "OpenHearthFurnace";
	}

	public Widget GetWidget(IInventory inventory, ComponentHearthFurnace component)
	{
		return new HearthFurnaceWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new HearthFurnaceDeviceBehavior(project)
		};
	}
}
