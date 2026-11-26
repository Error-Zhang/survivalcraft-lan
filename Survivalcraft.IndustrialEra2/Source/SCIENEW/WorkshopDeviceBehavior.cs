using GameEntitySystem;

namespace SCIENEW;

public class WorkshopDeviceBehavior : EntityDeviceBehavior<ComponentIECraftingTable>
{
	public WorkshopDeviceBehavior(Project project)
		: base(project)
	{
	}
}
