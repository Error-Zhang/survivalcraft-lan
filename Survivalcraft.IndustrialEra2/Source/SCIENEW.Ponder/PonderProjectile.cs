using Engine;
using Game;

namespace SCIENEW.Ponder;

public class PonderProjectile : Projectile
{
	public required Ponder Ponder { get; set; }

	public ProjectilePonderBlock BelongingBlock { get; set; }

	public override void TurnIntoPickable(Vector3? pickableStuckMatrix)
	{
		Block projectileBlock = BlocksManager.Blocks[Terrain.ExtractContents(Value)];
		int num = BlocksManager.DamageItem(Value, DamageToPickable, OwnerEntity);
		if (TurnIntoPickableBlockValue.HasValue)
		{
			num = TurnIntoPickableBlockValue.Value;
		}
		if (num != 0)
		{
			if (pickableStuckMatrix.HasValue)
			{
				SubsystemProjectiles.CalculateVelocityAlignMatrix(projectileBlock, pickableStuckMatrix.Value, Velocity, out var matrix);
				Ponder.PickablePonderBlockSet.Add(num, Position, Vector3.Zero, matrix, BelongingBlock.Tag);
			}
			else
			{
				Ponder.PickablePonderBlockSet.Add(num, Position, Vector3.Zero, null, BelongingBlock.Tag);
			}
		}
		ToRemove = true;
	}
}
