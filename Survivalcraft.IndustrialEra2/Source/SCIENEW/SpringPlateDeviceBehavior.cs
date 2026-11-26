using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Utils;

namespace SCIENEW;

public class SpringPlateDeviceBehavior : EntityDeviceBehavior<ComponentSpringPlate>
{
	public SpringPlateDeviceBehavior(Project project)
		: base(project)
	{
	}

	public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
	{
		return false;
	}

	public override void OnCollide(CellFace cellFace, float velocity, ComponentBody componentBody)
	{
		base.OnCollide(cellFace, velocity, componentBody);
		if (cellFace.Face == 4)
		{
			Vector3 velocity2 = componentBody.Velocity;
			if (!(velocity2.Y > -2f) && BlockEntityUtils.GetBlockEntity(Project.FindSubsystem<SubsystemTerrain>(throwOnError: true), cellFace.Point, out ComponentBlockEntity blockEntity))
			{
				blockEntity.Entity.FindComponent<ComponentSpringPlate>(throwOnError: true).OnJump();
				componentBody.ApplyImpulse(new Vector3(velocity2.X, 0f - velocity2.Y, velocity2.Z));
			}
		}
	}
}
