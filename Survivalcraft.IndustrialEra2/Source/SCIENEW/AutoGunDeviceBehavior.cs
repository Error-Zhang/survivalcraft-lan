using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Utils;

namespace SCIENEW;

public class AutoGunDeviceBehavior : EntityDeviceBehavior<ComponentAutoGun>
{
	public AutoGunDeviceBehavior(Project project)
		: base(project)
	{
	}

	public override void OnBlockAdded(int value, int oldValue, int x, int y, int z)
	{
		base.OnBlockAdded(value, oldValue, x, y, z);
		if (BlockEntityUtils.GetBlockEntity(Project.FindSubsystem<SubsystemTerrain>(throwOnError: true), new Point3(x, y, z), out ComponentBlockEntity blockEntity))
		{
			blockEntity.Entity.FindComponent<ComponentTurretTargeting>()?.TryAssignOwnerFromNearestPlayer();
		}
	}
}
