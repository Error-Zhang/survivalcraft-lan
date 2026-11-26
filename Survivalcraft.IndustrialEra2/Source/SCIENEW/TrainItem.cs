using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class TrainItem : MeshItem
{
	public TrainItem()
		: base("Train", "Models/Train", "Cylinder", ContentManager.Get<Texture2D>("Textures/Train"), Color.White, Matrix.Identity, Matrix.Identity)
	{
	}

	public override Vector3 GetIconViewOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return new Vector3(-0.6f, 0.6f, -0.8f);
	}

	public override Vector3 GetIconBlockOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return new Vector3(0f, -0.2f, 0f);
	}

	public override ItemBehavior[] GetBehaviors(Project project)
	{
		return new ItemBehavior[1]
		{
			new TrainItemBehavior(project)
		};
	}
}
