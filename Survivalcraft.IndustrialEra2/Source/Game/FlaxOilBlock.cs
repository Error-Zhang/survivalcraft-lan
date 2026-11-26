using Engine;

namespace Game;

public class FlaxOilBlock : FluidBlock
{
	public static int Index = 525;

	public new static int MaxLevel = 7;

	public FlaxOilBlock()
		: base(MaxLevel)
	{
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		Color color = new Color(255, 200, 0);
		Color color2 = color;
		color.A = byte.MaxValue;
		Color topColor = color2;
		topColor.A = 0;
		GenerateFluidTerrainVertices(generator, value, x, y, z, color, topColor, geometry.TransparentSubsetsByFace);
	}
}
