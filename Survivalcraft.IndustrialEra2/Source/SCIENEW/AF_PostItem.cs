using Engine.Graphics;

namespace SCIENEW;

public class AF_PostItem : FlatItem
{
	public Texture2D m_texture;

	public AF_PostItem(string id, Texture2D texture)
		: base(id)
	{
		m_texture = texture;
	}

	public override Texture2D GetTexture()
	{
		return m_texture;
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 10086;
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return 0;
	}

	public override int GetTextureSlotCount(int value)
	{
		return 1;
	}

	public override int GetMaxStacking(int value)
	{
		return int.MaxValue;
	}

	public override bool IsDisplay()
	{
		return IndustrialModLoader.IsAprilFools;
	}
}
