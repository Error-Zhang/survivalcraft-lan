using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class SparkPlugItem : MeshItem
{
	public SparkPlugItem()
		: base("SparkPlug", "Models/SparkPlug", "SparkPlug", ContentManager.Get<Texture2D>("Textures/SparkPlug"), Color.White, Matrix.CreateTranslation(0f, 0.4f, 0f), Matrix.Identity)
	{
	}

	public override float GetIconViewScale(int value, DrawBlockEnvironmentData environmentData)
	{
		return 1.4f;
	}
}
