using Engine;

namespace SCIENEW;

public class CokeCoalNBlock : StoneTypeNBlock
{
	public CokeCoalNBlock()
		: base("CokeCoalCube", IndustrialModLoader.IETexture, new Color(135, 135, 135), 113)
	{
	}

	public override float GetFuelHeatLevel(int value)
	{
		return 3f;
	}

	public override float GetFuelFireDuration(int value)
	{
		return 1000f;
	}
}
