using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class MercuryBlock : FocalorsFluidBlock
{
	public static int Index = 526;

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		Color white;
		Color topColor = (white = Color.White);
		GenerateFluidTerrainVertices(generator, value, x, y, z, white, topColor, geometry.OpaqueSubsetsByFace);
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		DrawFocalorsFluidBlock(primitivesRenderer, value, color, size, ref matrix, environmentData);
	}
}
