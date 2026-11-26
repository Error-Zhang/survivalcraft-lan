using Engine.Graphics;

namespace SCIENEW;

public class IEFlatItem : FlatItem
{
	public IEFlatItem(string id, int textureSlot)
		: base(id, textureSlot)
	{
	}

	public override Texture2D GetTexture()
	{
		return IndustrialModLoader.IETexture;
	}
}
