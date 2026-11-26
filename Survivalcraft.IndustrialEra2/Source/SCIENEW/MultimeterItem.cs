using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class MultimeterItem : MeshItem
{
	public MultimeterItem()
		: base("Multimeter", "Models/Multimeter", "Multimeter", ContentManager.Get<Texture2D>("Textures/Multimeter"), Color.White, Matrix.Identity, Matrix.Identity, 1.5f)
	{
	}

	public override string GetCategory(int value)
	{
		return "Tools";
	}

	public override Vector3 GetIconViewOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return new Vector3(-1f, 0f, 0f);
	}

	public override ItemBehavior[] GetBehaviors(Project project)
	{
		return new ItemBehavior[1]
		{
			new MultimeterItemBehavior(project)
		};
	}

	public override float GetIconViewScale(int value, DrawBlockEnvironmentData environmentData)
	{
		return 1.2f;
	}
}
