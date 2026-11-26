using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class OilSludgeNBlock : DefaultNBlock
{
	public OilSludgeNBlock()
		: base("OilSludge")
	{
		Color = new Color(36, 28, 20);
		FaceTextureSlot = 0;
		Texture = IndustrialModLoader.BaseTexture;
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		generator.GenerateCubeVertices(BlocksManager.Blocks[BaseNormalBlock.Index], value, x, y, z, GetColor(value), geometry.GetGeometry(GetTexture(value)).OpaqueSubsetsByFace);
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawCubeBlock(primitivesRenderer, value, new Vector3(size), ref matrix, GetColor(value), GetColor(value) * color, environmentData, GetTexture(value));
	}

	public override bool IsCollidable_(int value)
	{
		return false;
	}

	public override bool IsFluidBlocker_(int value)
	{
		return true;
	}

	public override bool IsTransparent_(int value)
	{
		return true;
	}

	public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
	{
		return true;
	}

	public override bool ShouldGenerateFace(SubsystemTerrain subsystemTerrain, int face, int value, int neighborValue)
	{
		int num = Terrain.ExtractContents(neighborValue);
		if (BlocksManager.Blocks[num].IsFaceTransparent(subsystemTerrain, CellFace.OppositeFace(face), neighborValue))
		{
			return Terrain.ReplaceLight(neighborValue, 0) != NormalBlockManager.GetNormalBlockID(this);
		}
		return false;
	}

	public override string GetSoundMaterialName(SubsystemTerrain subsystemTerrain, int value)
	{
		return "Soft";
	}

	public override string GetCategory(int value)
	{
		return "Terrain";
	}

	public override float GetExplosionResilience(int value)
	{
		return 25f;
	}

	public override BlockDigMethod GetBlockDigMethod(int value)
	{
		return BlockDigMethod.Shovel;
	}

	public override int GetDisplayOrder(int value)
	{
		return 28;
	}
}
