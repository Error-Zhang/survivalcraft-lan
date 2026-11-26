using GameEntitySystem;

namespace SCIENEW;

public class CrusherDevice : SixDirectionalDevice
{
	public CrusherDevice()
		: base("CrusherDevice", 127, 111, 111)
	{
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new CrusherBehavior(project)
		};
	}
}
