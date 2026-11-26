using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Utils;

namespace SCIENEW;

public class AutoLaserDeviceBehavior : EntityDeviceBehavior<ComponentAutoLaser>
{
	public AutoLaserDeviceBehavior(Project project)
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

	public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
	{
		if (componentMiner.ComponentPlayer == null)
		{
			return false;
		}
		if (!BlockEntityUtils.GetBlockEntity(Project.FindSubsystem<SubsystemTerrain>(throwOnError: true), raycastResult.CellFace.Point, out ComponentBlockEntity blockEntity))
		{
			return false;
		}
		ComponentTurretTargeting componentTurretTargeting = blockEntity.Entity.FindComponent<ComponentTurretTargeting>();
		if (componentTurretTargeting == null)
		{
			return false;
		}
		DialogsManager.ShowDialog(componentMiner.ComponentPlayer.GuiWidget, new TurretTargetingDialog(componentTurretTargeting));
		AudioManager.PlaySound("Audio/UI/ButtonClick", 1f, 0f, 0f);
		return true;
	}
}
