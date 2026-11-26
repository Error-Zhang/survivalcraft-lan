using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class RailBlock : Block
{
	public static int Index = 517;

	internal static BoundingBox[][] boundingBoxes = new BoundingBox[10][];

	private static readonly BlockMesh[] m_blockMeshes = new BlockMesh[10];

	public override void Initialize()
	{
		base.Initialize();
		BlockMesh blockMesh = new BlockMesh();
		int num = 4;
		List<BlockMeshVertex> list = new List<BlockMeshVertex>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<BlockMeshVertex> span = CollectionsMarshal.AsSpan(list);
		int num2 = 0;
		span[num2] = new BlockMeshVertex
		{
			Color = Color.White,
			TextureCoordinates = new Vector2(0f, 0.0625f),
			Position = new Vector3(0f, 0.01f, 1f)
		};
		num2++;
		span[num2] = new BlockMeshVertex
		{
			Color = Color.White,
			TextureCoordinates = new Vector2(0.0625f, 0.0625f),
			Position = new Vector3(1f, 0.01f, 1f)
		};
		num2++;
		span[num2] = new BlockMeshVertex
		{
			Color = Color.White,
			TextureCoordinates = new Vector2(0.0625f, 0f),
			Position = new Vector3(1f, 0.01f, 0f)
		};
		num2++;
		span[num2] = new BlockMeshVertex
		{
			Color = Color.White,
			TextureCoordinates = new Vector2(0f, 0f),
			Position = new Vector3(0f, 0.01f, 0f)
		};
		IList<BlockMeshVertex> items = list;
		num2 = 6;
		List<int> list2 = new List<int>(num2);
		CollectionsMarshal.SetCount(list2, num2);
		Span<int> span2 = CollectionsMarshal.AsSpan(list2);
		num = 0;
		span2[num] = 2;
		num++;
		span2[num] = 1;
		num++;
		span2[num] = 0;
		num++;
		span2[num] = 0;
		num++;
		span2[num] = 3;
		num++;
		span2[num] = 2;
		IList<int> items2 = list2;
		blockMesh.Vertices.AddRange(items);
		blockMesh.Indices.AddRange(items2);
		BlockMesh blockMesh2 = new BlockMesh();
		blockMesh2.AppendBlockMesh(blockMesh);
		blockMesh2.TransformTextureCoordinates(Matrix.CreateTranslation(new Vector3(0.875f, 0.875f, 0f)));
		Matrix matrix = Matrix.CreateTranslation(new Vector3(0.5f, 0f, 0.5f));
		Matrix matrix2 = Matrix.CreateTranslation(new Vector3(-0.5f, 0f, -0.5f));
		for (int i = 0; i < 4; i++)
		{
			m_blockMeshes[i] = new BlockMesh();
			m_blockMeshes[i].AppendBlockMesh(blockMesh2);
			m_blockMeshes[i].TransformPositions(matrix2 * Matrix.CreateRotationY((float)Math.PI / 2f * (float)i) * matrix);
			m_blockMeshes[i].GenerateSidesData();
		}
		blockMesh2 = new BlockMesh();
		blockMesh2.AppendBlockMesh(blockMesh);
		blockMesh2.TransformTextureCoordinates(Matrix.CreateTranslation(new Vector3(0.8125f, 0.875f, 0f)));
		m_blockMeshes[4] = new BlockMesh();
		m_blockMeshes[4].AppendBlockMesh(blockMesh2);
		m_blockMeshes[4].GenerateSidesData();
		m_blockMeshes[5] = new BlockMesh();
		m_blockMeshes[5].AppendBlockMesh(blockMesh2);
		m_blockMeshes[5].TransformPositions(matrix2 * Matrix.CreateRotationY((float)Math.PI / 2f) * matrix);
		m_blockMeshes[5].GenerateSidesData();
		blockMesh = new BlockMesh();
		num = 4;
		List<BlockMeshVertex> list3 = new List<BlockMeshVertex>(num);
		CollectionsMarshal.SetCount(list3, num);
		Span<BlockMeshVertex> span3 = CollectionsMarshal.AsSpan(list3);
		num2 = 0;
		span3[num2] = new BlockMeshVertex
		{
			Color = Color.White,
			TextureCoordinates = new Vector2(0f, 0.0625f),
			Position = new Vector3(0f, 0.01f, 1f)
		};
		num2++;
		span3[num2] = new BlockMeshVertex
		{
			Color = Color.White,
			TextureCoordinates = new Vector2(0.0625f, 0.0625f),
			Position = new Vector3(1f, 0.01f, 1f)
		};
		num2++;
		span3[num2] = new BlockMeshVertex
		{
			Color = Color.White,
			TextureCoordinates = new Vector2(0.0625f, 0f),
			Position = new Vector3(1f, 1.01f, 0f)
		};
		num2++;
		span3[num2] = new BlockMeshVertex
		{
			Color = Color.White,
			TextureCoordinates = new Vector2(0f, 0f),
			Position = new Vector3(0f, 1.01f, 0f)
		};
		items = list3;
		num2 = 12;
		List<int> list4 = new List<int>(num2);
		CollectionsMarshal.SetCount(list4, num2);
		Span<int> span4 = CollectionsMarshal.AsSpan(list4);
		num = 0;
		span4[num] = 2;
		num++;
		span4[num] = 1;
		num++;
		span4[num] = 0;
		num++;
		span4[num] = 0;
		num++;
		span4[num] = 1;
		num++;
		span4[num] = 2;
		num++;
		span4[num] = 0;
		num++;
		span4[num] = 3;
		num++;
		span4[num] = 2;
		num++;
		span4[num] = 2;
		num++;
		span4[num] = 3;
		num++;
		span4[num] = 0;
		items2 = list4;
		blockMesh.Vertices.AddRange(items);
		blockMesh.Indices.AddRange(items2);
		blockMesh.TransformTextureCoordinates(Matrix.CreateTranslation(new Vector3(0.8125f, 0.875f, 0f)));
		for (int j = 6; j < 10; j++)
		{
			m_blockMeshes[j] = new BlockMesh();
			m_blockMeshes[j].AppendBlockMesh(blockMesh);
			m_blockMeshes[j].TransformPositions(matrix2 * Matrix.CreateRotationY((float)Math.PI / 2f * (float)j) * matrix);
		}
		BoundingBox[] array = new BoundingBox[1]
		{
			new BoundingBox(0f, 0f, 0f, 1f, 0.01f, 1f)
		};
		for (int k = 0; k < 6; k++)
		{
			boundingBoxes[k] = array;
		}
		boundingBoxes[6] = new BoundingBox[4]
		{
			new BoundingBox(0f, 0f, 0f, 1f, 0.01f, 1f),
			new BoundingBox(0f, 0f, 0.25f, 1f, 0.25f, 1f),
			new BoundingBox(0f, 0f, 0.5f, 1f, 0.5f, 1f),
			new BoundingBox(0f, 0f, 0.75f, 1f, 0.75f, 1f)
		};
		boundingBoxes[7] = new BoundingBox[4]
		{
			new BoundingBox(0f, 0f, 0f, 1f, 0.01f, 1f),
			new BoundingBox(0.25f, 0f, 0f, 1f, 0.25f, 1f),
			new BoundingBox(0.5f, 0f, 0f, 1f, 0.5f, 1f),
			new BoundingBox(0.75f, 0f, 0f, 1f, 0.75f, 1f)
		};
		boundingBoxes[8] = new BoundingBox[4]
		{
			new BoundingBox(0f, 0f, 0f, 1f, 0.01f, 1f),
			new BoundingBox(0f, 0f, 0f, 1f, 0.25f, 0.75f),
			new BoundingBox(0f, 0f, 0f, 1f, 0.5f, 0.5f),
			new BoundingBox(0f, 0f, 0f, 1f, 0.75f, 0.25f)
		};
		boundingBoxes[9] = new BoundingBox[4]
		{
			new BoundingBox(0f, 0f, 0f, 1f, 0.01f, 1f),
			new BoundingBox(0f, 0f, 0f, 0.75f, 0.25f, 1f),
			new BoundingBox(0f, 0f, 0f, 0.5f, 0.5f, 1f),
			new BoundingBox(0f, 0f, 0f, 0.25f, 0.75f, 1f)
		};
	}

	public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value)
	{
		return boundingBoxes[GetRailType(Terrain.ExtractData(value))];
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawFlatBlock(primitivesRenderer, value, size, ref matrix, IndustrialModLoader.IETexture, color, isEmissive: false, environmentData);
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		generator.GenerateMeshVertices(this, x, y, z, m_blockMeshes[GetRailType(Terrain.ExtractData(value))], Color.White, null, geometry.GetGeometry(IndustrialModLoader.IETexture).SubsetAlphaTest);
	}

	public static int GetRailType(int data)
	{
		return data & 0xF;
	}

	public static int SetRailType(int data, int type)
	{
		return (data & -16) | (type & 0xF);
	}

	public static bool IsCorner(int type)
	{
		return type >> 2 == 0;
	}

	public static bool IsDirectionX(int type)
	{
		return (type & 1) == 0;
	}

	public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
	{
		Vector3 forwardVector = componentMiner.ComponentCreature.ComponentCreatureModel.EyeRotation.GetForwardVector();
		BlockPlacementData result = new BlockPlacementData();
		result.CellFace = raycastResult.CellFace;
		result.Value = Terrain.MakeBlockValue(Index, 0, SetRailType(0, (Math.Abs(forwardVector.X) < Math.Abs(forwardVector.Z)) ? 4 : 5));
		return result;
	}

	public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris)
	{
		showDebris = true;
		dropValues.Add(new BlockDropValue
		{
			Value = Index,
			Count = 1
		});
	}

	public static bool CanConnectTo(int railType, int direction)
	{
		if (IsCorner(railType))
		{
			if (direction != railType)
			{
				return direction == ((railType + 1) & 3);
			}
			return true;
		}
		return IsDirectionX(railType) ^ !IsDirectionX(direction);
	}
}
