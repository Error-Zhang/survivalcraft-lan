using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class WireItem : FlatItem
{
	public WireItem(string craftingId)
		: base(craftingId)
	{
		DefaultTextureSlot = 213;
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawFlatBlock(primitivesRenderer, value, size, ref matrix, IndustrialModLoader.IETexture, color, isEmissive: false, environmentData);
	}
}
