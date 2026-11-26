using System;
using Engine;

namespace SCIENEW.IconMaker;

public class IconMakerProject
{
	public const int DefaultOutputSize = 1024;

	public const int MinOutputSize = 128;

	public const int MaxOutputSize = 2048;

	public const int DefaultBlockValue = 1049079;

	public int OutputSize = 1024;

	public IconMakerSubjectKind SubjectKind;

	public int BlockValue = 1049079;

	public string TexturePath = "Textures/White";

	public float SubjectScale = 0.85f;

	public Vector2 SubjectOffset;

	public bool OutlineEnabled = true;

	public float OutlineWidth = 5f;

	public Color OutlineColor = ParseRgb("89A3BF");

	public bool GlowEnabled = true;

	public IconMakerGlowMode GlowMode;

	public float GlowRadius = 48f;

	public float GlowIntensity = 1.5f;

	public Color GlowColor = ParseRgb("C5D2FF");

	public Vector2 GlowOffset;

	public string BackgroundTexturePath = "Textures/M-SolarPanel";

	public IconMakerBgSamplerMode BgSamplerMode;

	public Vector2 BgCropMin;

	public Vector2 BgCropMax = Vector2.One;

	public float BgTileScale = 4f;

	public float BgBlur = 7.5f;

	public float BgBrightness = 0.2f;

	public float BgContrast;

	public float BgSaturation = 1f;

	public float BgVibrance = 1f;

	private static Color ParseRgb(string hex)
	{
		hex = hex.TrimStart('#');
		int r = Convert.ToInt32(hex.Substring(0, 2), 16);
		int g = Convert.ToInt32(hex.Substring(2, 2), 16);
		int b = Convert.ToInt32(hex.Substring(4, 2), 16);
		return new Color(r, g, b);
	}
}
