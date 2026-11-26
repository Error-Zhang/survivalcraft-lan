using System;
using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class DefaultNBlock : NormalBlock
{
	public Color Color;

	public Texture2D Texture;

	public string DisplayName = string.Empty;

	public string Description = string.Empty;

	public string Id = string.Empty;

	public int FaceTextureSlot;

	public DefaultNBlock(string id)
	{
		Id = id;
		try
		{
			DisplayName = LanguageControl.Get("NormalBlocks", "Names", GetCraftingId());
			Description = LanguageControl.Get("NormalBlocks", "Descriptions", GetCraftingId());
		}
		catch (NullReferenceException)
		{
		}
	}

	public override string GetCraftingId()
	{
		return Id;
	}

	public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
	{
		return DisplayName;
	}

	public override string GetDescription(int value)
	{
		return Description;
	}

	public override Color GetColor(int value)
	{
		return Color;
	}

	public override Texture2D GetTexture(int value)
	{
		return Texture;
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return FaceTextureSlot;
	}

	public override BlockDebrisParticleSystem CreateDebrisParticleSystem(SubsystemTerrain subsystemTerrain, Vector3 position, int value, float strength)
	{
		return new BlockDebrisParticleSystem(subsystemTerrain, position, 1f, 1f, Color, FaceTextureSlot, Texture);
	}

	public DefaultNBlock(string id, Texture2D texture, Color color, int faceTextureSlot)
		: this(id)
	{
		Texture = texture;
		Color = color;
		FaceTextureSlot = faceTextureSlot;
	}
}
