using Engine;

namespace SCIENEW.Ponder;

public class ProjectilePonderBlock : IPonderBlock
{
	public PonderProjectile Projectile;

	public int Value => Projectile.Value;

	public Vector3 Position => Projectile.Position;

	public object Tag { get; set; }

	public ProjectilePonderBlock(PonderProjectile projectile, object tag)
	{
		Projectile = projectile;
		Projectile.BelongingBlock = this;
		Tag = tag;
	}
}
