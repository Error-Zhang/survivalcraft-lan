using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class SixDirectionModelDevice : DefaultDevice, IRotatableDevice
{
	public Model m_model;

	public BlockMesh m_standaloneBlockMesh = new BlockMesh();

	public BlockMesh[] m_blockMeshesByFace = new BlockMesh[6];

	public BoundingBox[][] m_collisionBoxesByFace = new BoundingBox[6][];

	public Texture2D m_texture;

	public bool m_faceTransparent;

	public SixDirectionModelDevice(string id, string modelPath, string meshName, bool faceTransparent = false, bool useStandaloneMesh = true)
		: base(id)
	{
		m_model = ContentManager.Get<Model>(modelPath);
		Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(m_model.FindMesh(meshName).ParentBone);
		for (int i = 0; i < 6; i++)
		{
			Matrix matrix = ((i >= 4) ? ((i != 4) ? (Matrix.CreateRotationX((float)Math.PI) * Matrix.CreateTranslation(0.5f, 1f, 0.5f)) : Matrix.CreateTranslation(0.5f, 0f, 0.5f)) : (Matrix.CreateRotationX((float)Math.PI / 2f) * Matrix.CreateTranslation(0f, 0f, -0.5f) * Matrix.CreateRotationY((float)i * (float)Math.PI / 2f) * Matrix.CreateTranslation(0.5f, 0.5f, 0.5f)));
			m_blockMeshesByFace[i] = new BlockMesh();
			m_blockMeshesByFace[i].AppendModelMeshPart(m_model.FindMesh(meshName).MeshParts[0], boneAbsoluteTransform * matrix, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
			m_collisionBoxesByFace[i] = new BoundingBox[1] { m_blockMeshesByFace[i].CalculateBoundingBox() };
		}
		if (useStandaloneMesh)
		{
			m_standaloneBlockMesh.AppendModelMeshPart(m_model.FindMesh(meshName).MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0f, -0.5f, 0f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		}
		else
		{
			m_standaloneBlockMesh.AppendBlockMesh(m_blockMeshesByFace[0]);
			m_standaloneBlockMesh.TransformPositions(Matrix.CreateTranslation(-0.5f, -0.5f, -0.5f));
		}
		m_faceTransparent = faceTransparent;
		m_texture = IndustrialModLoader.IETexture;
	}

	public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value)
	{
		return m_collisionBoxesByFace[GetDirection(value)];
	}

	public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
	{
		return m_faceTransparent;
	}

	protected virtual int GetDirection(int value)
	{
		return GetDirection_Static(value);
	}

	public static int GetDirection_Static(int value)
	{
		return DevicesUtils.GetSixFaceDirection(value);
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		generator.GenerateShadedMeshVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], x, y, z, m_blockMeshesByFace[GetDirection(value)], Color.White, null, null, geometry.GetGeometry(m_texture).SubsetAlphaTest);
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, m_texture, color, size, ref matrix, environmentData);
	}

	public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
	{
		BlockPlacementData result = new BlockPlacementData();
		result.CellFace = raycastResult.CellFace;
		result.Value = DevicesBlockManager.GetDeviceValue(Id, DevicesUtils.GetDirectionXYZ(componentMiner));
		return result;
	}

	public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris)
	{
		showDebris = true;
		dropValues.Add(new BlockDropValue
		{
			Value = DevicesBlockManager.GetDeviceValue(Id, 0),
			Count = 1
		});
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return 105;
	}

	public int GetNextDirection(int value, bool reverse = false)
	{
		return DevicesBlockManager.GetDeviceValue(Id, (DevicesUtils.GetSixFaceDirection(value) + (reverse ? 5 : 1)) % 6);
	}
}
