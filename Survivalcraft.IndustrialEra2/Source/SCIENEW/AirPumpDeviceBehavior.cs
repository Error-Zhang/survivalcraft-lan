using Game;
using GameEntitySystem;

namespace SCIENEW;

public class AirPumpDeviceBehavior : EntityDeviceBehavior<ComponentAirPump>
{
	public AirPumpDeviceBehavior(Project project)
		: base(project)
	{
	}

	public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
	{
		return false;
	}

	public override void OnHitByProjectile(CellFace cellFace, WorldItem worldItem)
	{
	}
}
