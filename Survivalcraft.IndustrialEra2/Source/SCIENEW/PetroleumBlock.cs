using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class PetroleumBlock : FluidBlock
{
	public static int Index = 524;

	public new static int MaxLevel = 4;

	public PetroleumBlock()
		: base(MaxLevel)
	{
	}

	public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
	{
		return true;
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		GenerateFluidTerrainVertices(generator, value, x, y, z, Color.DarkGray, Color.DarkGray, geometry.OpaqueSubsetsByFace);
	}

	public override bool ShouldAvoid(int value)
	{
		return true;
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawCubeBlock(primitivesRenderer, value, new Vector3(size), ref matrix, color * Color.DarkGray, color * Color.DarkGray, environmentData);
	}
}
