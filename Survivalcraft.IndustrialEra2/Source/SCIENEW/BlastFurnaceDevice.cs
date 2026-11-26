using Game;
using GameEntitySystem;

namespace SCIENEW;

public class BlastFurnaceDevice : IlluminableDevice, IEntityDevice<ComponentBlastFurnace>
{
	public BlastFurnaceDevice()
		: base("BlastFurnaceDevice", 219, 225, 70, 70, 70)
	{
		DownSlot = 70;
	}

	public override string GetSoundMaterialName(SubsystemTerrain subsystemTerrain, int value)
	{
		return "Stone";
	}

	public string GetEntityName(int value)
	{
		return "BlastFurnace";
	}

	public Widget GetWidget(IInventory inventory, ComponentBlastFurnace component)
	{
		return new BlastFurnaceWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new BlastFurnaceDeviceBehavior(project)
		};
	}
}
