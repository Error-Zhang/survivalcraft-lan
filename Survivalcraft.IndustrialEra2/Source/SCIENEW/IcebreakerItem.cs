using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class IcebreakerItem : MeshItem
{
	public IcebreakerItem()
		: base("Icebreaker", "Models/SteamBoat", "Cylinder", ContentManager.Get<Texture2D>("Textures/SteamBoat"), Color.White, Matrix.Identity, Matrix.Identity)
	{
	}

	public override Vector3 GetIconViewOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return new Vector3(-0.7f, 0.4f, -1f);
	}

	public override float GetIconViewScale(int value, DrawBlockEnvironmentData environmentData)
	{
		return 0.5f;
	}

	public override bool IsDisplay()
	{
		return false;
	}
}
