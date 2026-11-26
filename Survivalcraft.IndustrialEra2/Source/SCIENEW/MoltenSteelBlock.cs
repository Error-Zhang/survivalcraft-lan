using Engine;
using Engine.Graphics;
using Game;
using SCIENEW.Extensions.GameLogic;

namespace SCIENEW;

public class MoltenSteelBlock : FocalorsFluidBlock
{
	public static int Index = 520;

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		Color metalColor;
		Color topColor = (metalColor = Materials.Steel.GetMetalColor());
		GenerateFluidTerrainVertices(generator, value, x, y, z, metalColor, topColor, geometry.OpaqueSubsetsByFace);
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		DrawFocalorsFluidBlock(primitivesRenderer, value, color * Materials.Steel.GetMetalColor(), size, ref matrix, environmentData);
	}

	public override int GetEmittedLightAmount(int value)
	{
		return 15;
	}
}
