using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class AF_CardPaperItem : MeshItem
{
	public AF_CardPaperItem()
		: base("AF_CardPaper", "Models/AF_CardPaper", "AF_CardPaper", ContentManager.Get<Texture2D>("Textures/AF_BoxGenerator"), Color.White, Matrix.CreateTranslation(0f, 0.2f, 0f), Matrix.Identity)
	{
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 10086;
	}

	public override bool IsDisplay()
	{
		return IndustrialModLoader.IsAprilFools;
	}
}
