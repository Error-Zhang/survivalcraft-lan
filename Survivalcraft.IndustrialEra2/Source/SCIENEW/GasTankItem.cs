using Engine;

namespace SCIENEW;

public class GasTankItem : MeshItem
{
	public GasTankItem(string id)
		: base(id, "Models/GasTank", "GasTank", IndustrialModLoader.IETexture, Color.LightGray, Matrix.CreateTranslation(0f, -0.13f, 0f), Matrix.Identity, 1.4f)
	{
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 2;
	}
}
