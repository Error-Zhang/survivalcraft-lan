using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions.GameLogic;

namespace SCIENEW;

public class WrenchsItem : MeshItem
{
	public WrenchsItem()
		: base("Wrench", "Models/Wrench", "obj1", ContentManager.Get<Texture2D>("Textures/Iron"), Materials.Steel.GetColor(), Matrix.CreateTranslation(0f, 5f / 12f, 0f), Matrix.Identity, 6f)
	{
	}

	public override ItemBehavior[] GetBehaviors(Project project)
	{
		return new ItemBehavior[1]
		{
			new ToolsItemBehavior(project)
		};
	}

	public override string GetCategory(int value)
	{
		return "Tools";
	}
}
