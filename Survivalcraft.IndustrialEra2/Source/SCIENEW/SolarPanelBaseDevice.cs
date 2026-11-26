using Game;
using GameEntitySystem;

namespace SCIENEW;

public class SolarPanelBaseDevice : FourDirectionalDevice, IEntityDevice<ComponentSolarPanelBase>
{
	public SolarPanelBaseDevice()
		: base("SolarPanelBaseDevice", 99, 99, 99, 147, 99)
	{
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 1;
	}

	public string GetEntityName(int value)
	{
		return "SolarPanelBase";
	}

	public Widget GetWidget(IInventory inventory, ComponentSolarPanelBase component)
	{
		return new SolarPanelBaseWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new SolarPanelBaseDeviceBehavior(project)
		};
	}

	public override bool IsDisplay()
	{
		return false;
	}
}
