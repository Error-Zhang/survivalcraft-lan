using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class SteamBoatItem : MeshItem
{
	public SteamBoatItem()
		: base("SteamBoat", "Models/SteamBoat", "Cylinder", ContentManager.Get<Texture2D>("Textures/SteamBoat"), Color.White, Matrix.Identity, Matrix.Identity)
	{
	}

	public override Vector3 GetIconBlockOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return new Vector3(0f, 0f, -0.3f);
	}

	public override Vector3 GetIconViewOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return new Vector3(-0.7f, 0.8f, -1f);
	}

	public override float GetIconViewScale(int value, DrawBlockEnvironmentData environmentData)
	{
		return 0.45f;
	}

	public override ItemBehavior[] GetBehaviors(Project project)
	{
		return new ItemBehavior[1]
		{
			new SteamBoatItemBehavior(project)
		};
	}
}
