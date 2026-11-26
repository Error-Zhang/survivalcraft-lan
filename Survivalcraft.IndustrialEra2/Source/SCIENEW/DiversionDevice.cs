using Game;
using GameEntitySystem;

namespace SCIENEW;

public class DiversionDevice : SixDirectionalDevice
{
	public DiversionDevice()
		: base("DiversionDevice", 59, 186, 186)
	{
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new DiversionBlockBehavior(project)
		};
	}

	public override string GetSoundMaterialName(SubsystemTerrain subsystemTerrain, int value)
	{
		return "Wood";
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) - 1;
	}
}
