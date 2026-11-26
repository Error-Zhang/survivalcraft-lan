using Engine;
using Engine.Graphics;
using Game;
using SCIENEW.Extensions.GameLogic;

namespace SCIENEW;

public class UIronSheetItem : MeshItem, IMaterialBlock
{
	public UIronSheetItem()
		: base("UIronSheet", "Models/UIronSheet", "UIronSheet", ContentManager.Get<Texture2D>("Textures/Iron"), Materials.Iron.GetColor(), Matrix.CreateScale(1.5f) * Matrix.CreateTranslation(0f, 0.5f, 0f), Matrix.Identity)
	{
	}

	public Materials GetMaterial()
	{
		return Materials.Iron;
	}

	public float GetMoltedVolume()
	{
		return 2f;
	}
}
