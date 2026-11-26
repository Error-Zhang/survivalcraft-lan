using Game;
using GameEntitySystem;

namespace SCIENEW;

public class FireBoxDevice : IlluminableDevice, IEntityDevice<ComponentFireBox>
{
	public FireBoxDevice()
		: base("FireBoxDevice", 222, 223, 221, 221, 221)
	{
		DownSlot = 221;
	}

	public string GetEntityName(int value)
	{
		return "FireBox";
	}

	public Widget GetWidget(IInventory inventory, ComponentFireBox component)
	{
		return new FireBoxWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new FireBoxDeviceBehavior(project)
		};
	}
}
