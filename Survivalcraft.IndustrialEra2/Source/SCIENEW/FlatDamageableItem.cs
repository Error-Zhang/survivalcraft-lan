using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class FlatDamageableItem : DefaultDamageableItem
{
	public int DefaultTextureSlot;

	public Color Color = Color.White;

	public int Durability;

	public FlatDamageableItem(string id, int textureSlot, Color color, int durability)
		: base(id)
	{
		DefaultTextureSlot = textureSlot;
		Color = color;
		Durability = durability;
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return DefaultTextureSlot;
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawFlatBlock(primitivesRenderer, value, size, ref matrix, IndustrialModLoader.IETexture, Color * color, isEmissive: false, environmentData);
	}

	public override Vector3 GetIconViewOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return new Vector3
		{
			Z = 1f
		};
	}

	public override int GetDurability(int value)
	{
		return Durability;
	}
}
