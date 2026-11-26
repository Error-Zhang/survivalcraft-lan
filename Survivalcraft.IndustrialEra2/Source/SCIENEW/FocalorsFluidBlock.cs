using System;
using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public abstract class FocalorsFluidBlock : CubeBlock
{
	public readonly BoundingBox[][] m_boundingBoxesByLevel = new BoundingBox[16][];

	public FocalorsFluidBlock()
	{
		for (int i = 0; i < 16; i++)
		{
			float y = 0.999f * MathUtils.Saturate(1f - (float)i / 16f);
			m_boundingBoxesByLevel[i] = new BoundingBox[1]
			{
				new BoundingBox(new Vector3(0f, 0f, 0f), new Vector3(1f, y, 1f))
			};
		}
	}

	public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value)
	{
		return m_boundingBoxesByLevel[(int)(GetLevel(Terrain.ExtractData(value)) * 15f)];
	}

	public static float GetLevelHeight(float level)
	{
		return 1f - level;
	}

	public void GenerateFluidTerrainVertices(BlockGeometryGenerator generator, int value, int x, int y, int z, Color sideColor, Color topColor, TerrainGeometrySubset[] subset)
	{
		int data = Terrain.ExtractData(value);
		if (GetIsTop(data))
		{
			Terrain terrain = generator.Terrain;
			int cellValueFast = terrain.GetCellValueFast(x - 1, y, z - 1);
			int cellValueFast2 = terrain.GetCellValueFast(x, y, z - 1);
			int cellValueFast3 = terrain.GetCellValueFast(x + 1, y, z - 1);
			int cellValueFast4 = terrain.GetCellValueFast(x - 1, y, z);
			int cellValueFast5 = terrain.GetCellValueFast(x + 1, y, z);
			int cellValueFast6 = terrain.GetCellValueFast(x - 1, y, z + 1);
			int cellValueFast7 = terrain.GetCellValueFast(x, y, z + 1);
			int cellValueFast8 = terrain.GetCellValueFast(x + 1, y, z + 1);
			float h = CalculateNeighborHeight(cellValueFast);
			float num = CalculateNeighborHeight(cellValueFast2);
			float h2 = CalculateNeighborHeight(cellValueFast3);
			float num2 = CalculateNeighborHeight(cellValueFast4);
			float num3 = CalculateNeighborHeight(cellValueFast5);
			float h3 = CalculateNeighborHeight(cellValueFast6);
			float num4 = CalculateNeighborHeight(cellValueFast7);
			float h4 = CalculateNeighborHeight(cellValueFast8);
			float levelHeight = GetLevelHeight(GetLevel(data));
			float height = CalculateFluidVertexHeight(h, num, num2, levelHeight);
			float height2 = CalculateFluidVertexHeight(num, h2, levelHeight, num3);
			float height3 = CalculateFluidVertexHeight(levelHeight, num3, num4, h4);
			float height4 = CalculateFluidVertexHeight(num2, levelHeight, h3, num4);
			float x2 = ZeroSubst(num3, levelHeight) - ZeroSubst(num2, levelHeight);
			float value2 = ZeroSubst(num4, levelHeight) - ZeroSubst(num, levelHeight);
			int overrideTopTextureSlot = DefaultTextureSlot - MathF.Sign(x2) - 16 * Math.Sign(value2);
			generator.GenerateCubeVertices(this, value, x, y, z, height, height2, height3, height4, sideColor, topColor, topColor, topColor, topColor, overrideTopTextureSlot, subset);
		}
		else
		{
			generator.GenerateCubeVertices(this, value, x, y, z, sideColor, subset);
		}
	}

	public static float ZeroSubst(float v, float subst)
	{
		if (v != 0f)
		{
			return v;
		}
		return subst;
	}

	public static float CalculateFluidVertexHeight(float h1, float h2, float h3, float h4)
	{
		return Math.Min(MathUtils.Max(h1, h2, h3, h4), 1f);
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		if (face < 4)
		{
			return DefaultTextureSlot + 16;
		}
		return DefaultTextureSlot;
	}

	public override bool ShouldGenerateFace(SubsystemTerrain subsystemTerrain, int face, int value, int neighborValue, int x, int y, int z)
	{
		if (Terrain.ExtractContents(neighborValue) != BlockIndex)
		{
			return base.ShouldGenerateFace(subsystemTerrain, face, value, neighborValue, x, y, z);
		}
		return false;
	}

	public void DrawFocalorsFluidBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawCubeBlock(primitivesRenderer, value, new Vector3(size, size * (1f - GetLevel(Terrain.ExtractData(value))), size), ref matrix, color, color, environmentData);
	}

	public float CalculateNeighborHeight(int value)
	{
		if (Terrain.ExtractContents(value) == BlockIndex)
		{
			int data = Terrain.ExtractData(value);
			if (!GetIsTop(data))
			{
				return 1f;
			}
			return GetLevelHeight(GetLevel(data));
		}
		return 0f;
	}

	public override bool IsHeatBlocker(int value)
	{
		return true;
	}

	public static float GetLevel(int data)
	{
		return (float)((data >> 2) & 0xFFFF) / 65535f;
	}

	public static int SetLevel(int data, float level)
	{
		return (data & -4) | ((int)(Math.Min(level, 1f) * 65535f) << 2);
	}

	public static bool GetIsTop(int data)
	{
		return (data & 1) != 0;
	}

	public static int SetIsTop(int data, bool isTop)
	{
		if (!isTop)
		{
			return data & -2;
		}
		return data | 1;
	}
}
