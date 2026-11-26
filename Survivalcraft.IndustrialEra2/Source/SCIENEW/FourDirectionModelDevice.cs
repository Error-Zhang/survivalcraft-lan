using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public abstract class FourDirectionModelDevice : DefaultDevice, IRotatableDevice
{
	public BlockMesh m_standaloneBlockMesh = new BlockMesh();

	public BlockMesh[] m_blockMeshesByData = new BlockMesh[4];

	public BoundingBox[][] m_collisionBoxesByFace = new BoundingBox[4][];

	public bool faceTransparent;

	public FourDirectionModelDevice(string id, string modelPath, string meshName, bool faceTransparent = false, bool doubleSided = false)
		: base(id)
	{
		Model model = ContentManager.Get<Model>(modelPath);
		Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh(meshName).ParentBone);
		for (int i = 0; i < 4; i++)
		{
			m_blockMeshesByData[i] = new BlockMesh();
			Matrix identity = Matrix.Identity;
			identity *= Matrix.CreateRotationY((float)i * (float)Math.PI / 2f) * Matrix.CreateTranslation(0.5f, 0f, 0.5f);
			m_blockMeshesByData[i].AppendModelMeshPart(model.FindMesh(meshName).MeshParts[0], boneAbsoluteTransform * identity, makeEmissive: false, flipWindingOrder: false, doubleSided, flipNormals: false, Color.White);
			m_collisionBoxesByFace[i] = new BoundingBox[1] { m_blockMeshesByData[i].CalculateBoundingBox() };
		}
		m_standaloneBlockMesh.AppendModelMeshPart(model.FindMesh(meshName).MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0f, -0.5f, 0f), makeEmissive: false, flipWindingOrder: false, doubleSided, flipNormals: false, Color.White);
		this.faceTransparent = faceTransparent;
	}

	public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
	{
		return faceTransparent;
	}

	protected virtual int GetDirection(int value)
	{
		return GetDirection_Static(value);
	}

	public static int GetDirection_Static(int value)
	{
		return DevicesUtils.GetInnerData(value) & 3;
	}

	protected virtual bool GetSpecialData(int value)
	{
		return DevicesUtils.GetInnerData(value) >> 2 == 1;
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		generator.GenerateShadedMeshVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], x, y, z, m_blockMeshesByData[GetDirection(value)], Color.White, null, null, geometry.GetGeometry(IndustrialModLoader.IETexture).SubsetAlphaTest);
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, IndustrialModLoader.IETexture, color, size, ref matrix, environmentData);
	}

	public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
	{
		BlockPlacementData result = new BlockPlacementData();
		result.CellFace = raycastResult.CellFace;
		result.Value = DevicesBlockManager.GetDeviceValue(Id, DevicesUtils.GetDirectionXZ(componentMiner));
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

	public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value)
	{
		return m_collisionBoxesByFace[GetDirection(value)];
	}

	public int GetNextDirection(int value, bool reverse = false)
	{
		return DevicesBlockManager.GetDeviceValue(Id, (GetDirection_Static(value) + 1) % 4);
	}
}
