using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions.GameLogic;

namespace SCIENEW;

public class ScrewDriversItem : MeshItem
{
	public bool m_reverse;

	public ScrewDriversItem(bool reverse)
		: base("Screwdriver" + (reverse ? "_Reverse" : string.Empty), "Models/Screwdriver", "obj1", ContentManager.Get<Texture2D>("Textures/Iron"), Materials.Steel.GetColor(), Matrix.CreateTranslation(0f, 0.06f, 0f), Matrix.Identity, 3.3f)
	{
		m_reverse = reverse;
	}

	public override ItemBehavior[] GetBehaviors(Project project)
	{
		return new ItemBehavior[1]
		{
			new ScrewDriversItemBehavior(project, m_reverse)
		};
	}

	public override string GetCategory(int value)
	{
		return "Tools";
	}

	public override bool IsDisplay()
	{
		return !m_reverse;
	}
}
