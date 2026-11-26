using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public abstract class PlantNBlock : DefaultNBlock
{
	public PlantNBlock(string id, Texture2D texture, Color color, int faceTextureSlot)
		: base(id, texture, color, faceTextureSlot)
	{
	}

	public override string GetCategory(int value)
	{
		return "Plants";
	}

	public override bool IsCollidable_(int value)
	{
		return false;
	}

	public override bool IsPlacementTransparent_(int value)
	{
		return true;
	}

	public override bool IsFluidBlocker_(int value)
	{
		return false;
	}

	public override bool IsTransparent_(int value)
	{
		return true;
	}

	public override int GetShadowStrength(int value)
	{
		return 0;
	}

	public override float GetObjectShadowStrength(int value)
	{
		return 0f;
	}

	public override int GetMaxStacking(int value)
	{
		return 40;
	}

	public override float GetDensity(int value)
	{
		return 0.5f;
	}

	public override float GetFuelHeatLevel(int value)
	{
		return 1f;
	}

	public override float GetFuelFireDuration(int value)
	{
		return 0.5f;
	}

	public override string GetSoundMaterialName(SubsystemTerrain subsystemTerrain, int value)
	{
		return "Plant";
	}

	public override float GetFireDuration(int value)
	{
		return 6f;
	}

	public override float GetExplosionResilience(int value)
	{
		return 2f;
	}

	public override BlockDigMethod GetBlockDigMethod(int value)
	{
		return BlockDigMethod.Hack;
	}

	public override float GetDigResilience(int value)
	{
		return 0f;
	}

	public override FoodType GetFoodType(int value)
	{
		return FoodType.Grass;
	}

	public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
	{
		return true;
	}

	public override Vector3 GetIconViewOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return new Vector3
		{
			Z = 1f
		};
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawFlatOrImageExtrusionBlock(primitivesRenderer, value, size, ref matrix, GetTexture(value), color * Color, isEmissive: false, environmentData);
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		generator.GenerateCrossfaceVertices(BlocksManager.Blocks[BaseNormalBlock.Index], value, x, y, z, Color.White, GetFaceTextureSlot(0, value), geometry.GetGeometry(GetTexture(value)).SubsetAlphaTest);
	}
}
