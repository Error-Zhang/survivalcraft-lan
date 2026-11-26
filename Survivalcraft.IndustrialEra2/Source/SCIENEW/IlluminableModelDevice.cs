using System;
using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public abstract class IlluminableModelDevice : FourDirectionModelDevice, IRotatableDevice
{
	public BlockMesh[] m_litBlockMeshesByData = new BlockMesh[4];

	public BoundingBox[][] m_litCollisionBoxesByFace = new BoundingBox[4][];

	public IlluminableModelDevice(string id, string modelPath, string meshName, string litMeshName, bool faceTransparent = false, bool doubleSided = false)
		: base(id, modelPath, meshName, faceTransparent, doubleSided)
	{
		Model model = ContentManager.Get<Model>(modelPath);
		Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh(litMeshName).ParentBone);
		for (int i = 0; i < 4; i++)
		{
			m_litBlockMeshesByData[i] = new BlockMesh();
			Matrix identity = Matrix.Identity;
			identity *= Matrix.CreateRotationY((float)i * (float)Math.PI / 2f) * Matrix.CreateTranslation(0.5f, 0f, 0.5f);
			m_litBlockMeshesByData[i].AppendModelMeshPart(model.FindMesh(litMeshName).MeshParts[0], boneAbsoluteTransform * identity, makeEmissive: false, flipWindingOrder: false, doubleSided, flipNormals: false, Color.White);
			m_litCollisionBoxesByFace[i] = new BoundingBox[1] { m_litBlockMeshesByData[i].CalculateBoundingBox() };
		}
	}

	protected static bool IsLight(int value)
	{
		return DevicesUtils.GetInnerData(value) >> 2 == 1;
	}

	public override int GetEmittedLightAmount(int value)
	{
		if (!IsLight(value))
		{
			return 0;
		}
		return 15;
	}

	public override float GetHeat(int value)
	{
		if (!IsLight(value))
		{
			return 0f;
		}
		return 0.66f;
	}

	public override int GetShadowStrength(int value)
	{
		if (!IsLight(value))
		{
			return base.GetShadowStrength(value);
		}
		return -99;
	}

	public override bool IsTransparent_(int value)
	{
		return IsLight(value);
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		generator.GenerateShadedMeshVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], x, y, z, IsLight(value) ? m_litBlockMeshesByData[GetDirection(value)] : m_blockMeshesByData[GetDirection(value)], Color.White, null, null, geometry.GetGeometry(IndustrialModLoader.IETexture).SubsetAlphaTest);
	}

	public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value)
	{
		if (!IsLight(value))
		{
			return m_collisionBoxesByFace[GetDirection(value)];
		}
		return m_litCollisionBoxesByFace[GetDirection(value)];
	}

	public new int GetNextDirection(int value, bool reverse = false)
	{
		return DevicesBlockManager.GetDeviceValue(Id, ((FourDirectionModelDevice.GetDirection_Static(value) + 1) % 4) | (IsLight(value) ? 4 : 0));
	}
}
