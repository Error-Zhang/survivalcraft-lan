using Game;
using GameEntitySystem;

namespace SCIENEW;

public class ElectricPreheaterDeviceBehavior : EntityDeviceBehavior<ComponentElectricPreheater>
{
	public ElectricPreheaterDeviceBehavior(Project project)
		: base(project)
	{
	}

	public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
	{
		return false;
	}
}
