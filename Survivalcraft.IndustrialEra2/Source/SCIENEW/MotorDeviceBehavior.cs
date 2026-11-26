using Game;
using GameEntitySystem;

namespace SCIENEW;

public class MotorDeviceBehavior : EntityDeviceBehavior<ComponentMotor>
{
	public MotorDeviceBehavior(Project project)
		: base(project)
	{
	}

	public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
	{
		return false;
	}
}
