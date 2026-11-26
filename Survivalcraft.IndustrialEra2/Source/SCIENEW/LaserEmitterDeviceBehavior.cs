using Game;
using GameEntitySystem;

namespace SCIENEW;

public class LaserEmitterDeviceBehavior : EntityDeviceBehavior<ComponentLaserEmitter>
{
	public LaserEmitterDeviceBehavior(Project project)
		: base(project)
	{
	}

	public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
	{
		return false;
	}
}
