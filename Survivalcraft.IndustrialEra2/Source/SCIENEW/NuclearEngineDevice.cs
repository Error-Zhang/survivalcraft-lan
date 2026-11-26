using Game;
using GameEntitySystem;

namespace SCIENEW;

public class NuclearEngineDevice : IlluminableDevice, IEntityDevice<ComponentNuclearEngine>
{
	public NuclearEngineDevice()
		: base("NuclearEngineDevice", 152, 153, 141, 141, 137, 137)
	{
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 2;
	}

	public string GetEntityName(int value)
	{
		return "NuclearEngine";
	}

	public Widget GetWidget(IInventory inventory, ComponentNuclearEngine component)
	{
		return new NuclearEngineWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new NuclearEngineDeviceBehavior(project)
		};
	}
}
