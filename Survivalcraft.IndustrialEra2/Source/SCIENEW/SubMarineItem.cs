using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class SubMarineItem : MeshItem
{
	public SubMarineItem()
		: base("SubMarine", "Models/SubMarine", "Body", ContentManager.Get<Texture2D>("Textures/SubMarine"), Color.LightMagenta, Matrix.Identity, Matrix.Identity, 0.5f)
	{
	}

	public override Vector3 GetIconViewOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return new Vector3(-0.6f, 0.6f, -0.8f);
	}

	public override bool IsDisplay()
	{
		return false;
	}

	public override ItemBehavior[] GetBehaviors(Project project)
	{
		return new ItemBehavior[1]
		{
			new SubMarineItemBehavior(project)
		};
	}
}
