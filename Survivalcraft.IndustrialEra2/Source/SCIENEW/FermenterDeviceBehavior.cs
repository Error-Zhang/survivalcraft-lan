using GameEntitySystem;

namespace SCIENEW;

public class FermenterDeviceBehavior : EntityDeviceBehavior<ComponentFermenter>
{
	public FermenterDeviceBehavior(Project project)
		: base(project)
	{
	}
}
