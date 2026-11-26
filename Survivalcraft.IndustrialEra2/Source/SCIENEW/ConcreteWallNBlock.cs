using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class ConcreteWallNBlock : StoneTypeNBlock, IPaintableBlock
{
	public int? m_colorIndex;

	public string m_pureID;

	public ConcreteWallNBlock(string id, Texture2D texture, Color color, int faceTex, int explosionResilience = 2000, int digResilience = 24, int? colorIndex = null)
		: base(GetColoredCraftingID(id, colorIndex), texture, color, faceTex, explosionResilience, digResilience)
	{
		m_pureID = id;
		m_colorIndex = colorIndex;
	}

	public int? GetPaintColor()
	{
		return m_colorIndex;
	}

	public int Paint(SubsystemTerrain subsystemTerrain, int value, int? color)
	{
		return NormalBlockManager.GetNormalBlockID(GetColoredCraftingID(m_pureID, color));
	}

	public static string GetColoredCraftingID(string id, int? color)
	{
		return id + (color.HasValue ? ("." + color.Value) : "");
	}

	public override string GetCategory(int value)
	{
		if (m_colorIndex.HasValue)
		{
			return "Painted";
		}
		return base.GetCategory(value);
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		generator.GenerateCubeVertices(BlocksManager.Blocks[BaseNormalBlock.Index], value, x, y, z, m_colorIndex.HasValue ? (Color * SubsystemPalette.GetColor(generator.SubsystemTerrain, m_colorIndex)) : Color, geometry.GetGeometry(GetTexture(value)).OpaqueSubsetsByFace);
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawCubeBlock(primitivesRenderer, value, new Vector3(size), ref matrix, m_colorIndex.HasValue ? (Color * color * SubsystemPalette.GetColor(environmentData, m_colorIndex)) : (Color * color), m_colorIndex.HasValue ? (Color * color * SubsystemPalette.GetColor(environmentData, m_colorIndex)) : (Color * color), environmentData, GetTexture(value));
	}

	public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
	{
		return SubsystemPalette.GetName(subsystemTerrain, m_colorIndex, LanguageControl.Get("NormalBlocks", "Names", m_pureID));
	}

	public override string GetDescription(int value)
	{
		return LanguageControl.Get("NormalBlocks", "Descriptions", m_pureID);
	}

	public override BlockDebrisParticleSystem CreateDebrisParticleSystem(SubsystemTerrain subsystemTerrain, Vector3 position, int value, float strength)
	{
		return new BlockDebrisParticleSystem(subsystemTerrain, position, 1f, 1f, m_colorIndex.HasValue ? (Color * SubsystemPalette.GetColor(subsystemTerrain, m_colorIndex)) : Color, FaceTextureSlot, Texture);
	}
}
