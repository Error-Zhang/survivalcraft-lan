using Game;
using GameEntitySystem;

namespace SCIENEW;

public class StirlingEngineDevice : IlluminableDevice, IEntityDevice<ComponentStirlingEngine>
{
	public StirlingEngineDevice()
		: base("StirlingEngineDevice", 171, 187)
	{
	}

	public string GetEntityName(int value)
	{
		return "StirlingEngine";
	}

	public Widget GetWidget(IInventory inventory, ComponentStirlingEngine component)
	{
		return new StirlingEngineWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new StirlingEngineDeviceBehavior(project)
		};
	}
}
