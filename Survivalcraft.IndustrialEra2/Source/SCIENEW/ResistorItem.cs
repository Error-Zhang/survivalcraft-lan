using Engine;
using Engine.Graphics;
using Game;
using SCIENEW.Extensions.GameLogic;

namespace SCIENEW;

public class ResistorItem : FlatItem, IMaterialBlock
{
	public Materials Type;

	public float GetMoltedVolume()
	{
		return 0f;
	}

	public ResistorItem(Materials type)
		: base(type.ToString() + "Resistor", 163)
	{
		Type = type;
		Color = new Color(55, 55, 55);
		DisplayName = string.Format(LanguageControl.Get("Items", "Names", "ResistorItem"), type.ToDisplayName());
		Description = string.Format(LanguageControl.Get("Items", "Descriptions", "ResistorItem"), type.ToDisplayName());
	}

	public Materials GetMaterial()
	{
		return Materials.Steel;
	}

	public override Texture2D GetTexture()
	{
		return IndustrialModLoader.BaseTexture;
	}
}
