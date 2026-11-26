using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class FlatItem : DefaultItem
{
	public int DefaultTextureSlot;

	public Color Color = Color.White;

	public float Size = 1f;

	public FlatItem(string id, int textureSlot, float size = 1f)
		: base(id)
	{
		DefaultTextureSlot = textureSlot;
		Size = size;
	}

	public FlatItem(string id)
		: base(id)
	{
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return DefaultTextureSlot;
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawFlatBlock(primitivesRenderer, value, size * Size, ref matrix, GetTexture(), Color * color, isEmissive: false, environmentData);
	}

	public override Vector3 GetIconViewOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return new Vector3
		{
			Z = 1f
		};
	}

	public virtual Texture2D GetTexture()
	{
		return IndustrialModLoader.BaseTexture;
	}
}
