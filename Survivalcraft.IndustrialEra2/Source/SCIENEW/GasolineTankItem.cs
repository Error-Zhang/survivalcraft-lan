using Engine;

namespace SCIENEW;

public class GasolineTankItem : MeshItem
{
	public bool m_isFull;

	public GasolineTankItem(bool isFull)
		: base("GasolineTank" + (isFull ? "_Full" : string.Empty), "Models/GasolineTank", "GasolineTank", IndustrialModLoader.IETexture, Color.White, Matrix.CreateTranslation(0f, 0.4f, 0f), Matrix.Identity)
	{
		m_isFull = isFull;
	}

	public override bool IsDisplay()
	{
		return !m_isFull;
	}

	public override string GetCategory(int value)
	{
		return "Tools";
	}

	public override Vector3 GetFirstPersonRotation(int value)
	{
		return new Vector3(0f, 220f, 0f);
	}

	public override Vector3 GetInHandRotation(int value)
	{
		return new Vector3(60f, 180f, 0f);
	}
}
