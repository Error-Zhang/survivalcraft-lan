using Engine;

namespace SCIENEW;

public class CokeCoalItem : OreChunkItem
{
	public CokeCoalItem()
		: base("CokeCoalChunk", Matrix.CreateRotationX(1f) * Matrix.CreateRotationZ(2f), Matrix.CreateTranslation(0.0625f, 0.4375f, 0f), new Color(175, 175, 175), smooth: false)
	{
	}

	public override string GetCategory(int value)
	{
		return "Items";
	}

	public override float GetFuelHeatLevel(int value)
	{
		return 3f;
	}

	public override float GetFuelFireDuration(int value)
	{
		return 100f;
	}
}
