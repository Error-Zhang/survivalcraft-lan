using Engine;
using Engine.Graphics;
using Game;
using SCIENEW.Extensions.GameLogic;

namespace SCIENEW;

public class FanItem : FlatItem, IMaterialBlock
{
	public Materials Material;

	public Materials GetMaterial()
	{
		return Material;
	}

	public float GetMoltedVolume()
	{
		return 8f;
	}

	public FanItem(Materials type)
		: base(type.ToString() + "Fan")
	{
		Material = type;
		DisplayName = string.Format(LanguageControl.Get("Items", "Names", "FanItem"), Material.ToDisplayName());
		DefaultTextureSlot = 178;
		type.ToDisplayName();
		Description = string.Format(LanguageControl.Get("Items", "Descriptions", "FanItem"), Material.ToDisplayName());
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawFlatBlock(primitivesRenderer, value, size, ref matrix, IndustrialModLoader.IETexture, new Color(155, 155, 155), isEmissive: false, environmentData);
	}
}
