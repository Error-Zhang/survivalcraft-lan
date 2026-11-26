using Game;
using GameEntitySystem;

namespace SCIENEW;

public class CRTMonitorDeviceBehavior : EntityDeviceBehavior<ComponentCRTMonitor>
{
	public CRTMonitorDeviceBehavior(Project project)
		: base(project)
	{
	}

	public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
	{
		return false;
	}
}
