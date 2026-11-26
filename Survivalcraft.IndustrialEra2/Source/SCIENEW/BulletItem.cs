using Engine.Graphics;
using GameEntitySystem;

namespace SCIENEW;

public class BulletItem : FlatItem
{
	public float ProjectilePower;

	public float ExplosionPressure;

	public Texture2D Texture;

	public bool Display;

	public BulletItem(string id, Texture2D texture, int textureSlot, float projectilePower, float explosionPressure, float size = 1f, bool isDisplay = true)
		: base(id, textureSlot, size)
	{
		ProjectilePower = projectilePower;
		ExplosionPressure = explosionPressure;
		Texture = texture;
		Display = isDisplay;
	}

	public override string GetCategory(int value)
	{
		return "Weapons";
	}

	public override float GetProjectilePower(int value)
	{
		return ProjectilePower;
	}

	public override float GetExplosionPressure(int value)
	{
		return ExplosionPressure;
	}

	public override Texture2D GetTexture()
	{
		return Texture;
	}

	public override ItemBehavior[] GetBehaviors(Project project)
	{
		return new ItemBehavior[1]
		{
			new ImpactExplosivesItemBehavior(project)
		};
	}

	public override bool IsDisplay()
	{
		return Display;
	}

	public override int GetMaxStacking(int value)
	{
		return 800;
	}
}
