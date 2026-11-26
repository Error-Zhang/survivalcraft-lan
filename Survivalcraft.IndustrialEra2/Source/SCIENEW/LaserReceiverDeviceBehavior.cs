using Game;
using GameEntitySystem;

namespace SCIENEW;

public class LaserReceiverDeviceBehavior : EntityDeviceBehavior<ComponentLaserReceiver>
{
	public LaserReceiverDeviceBehavior(Project project)
		: base(project)
	{
	}

	public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
	{
		return false;
	}
}
