using Engine.Graphics;

namespace SCIENEW;

public class SifterMeshItem : FlatItem
{
	public SifterMeshItem()
		: base("SifterMeshItem", 218)
	{
	}

	public override Texture2D GetTexture()
	{
		return IndustrialModLoader.IETexture;
	}
}
