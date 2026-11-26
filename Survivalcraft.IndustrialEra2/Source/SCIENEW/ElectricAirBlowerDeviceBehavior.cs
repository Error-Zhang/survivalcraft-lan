using Game;
using GameEntitySystem;

namespace SCIENEW;

public class ElectricAirBlowerDeviceBehavior : EntityDeviceBehavior<ComponentElectricAirBlower>
{
	public ElectricAirBlowerDeviceBehavior(Project project)
		: base(project)
	{
	}

	public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
	{
		return false;
	}
}
