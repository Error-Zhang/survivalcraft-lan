using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class CastingTroughDevice : FourDirectionModelDevice, IFluidTransferrerPipeDevice
{
	public BlockMesh[] m_portMeshByFaces = new BlockMesh[4];

	public BlockMesh m_displayBlockMesh = new BlockMesh();

	public CastingTroughDevice()
		: base("CastingTroughDevice", "Models/CastingTrough", "CastingTrough", faceTransparent: true, doubleSided: true)
	{
		Model model = ContentManager.Get<Model>("Models/CastingTrough");
		Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("CastingTrough_Port1").ParentBone);
		for (int i = 0; i < 4; i++)
		{
			m_portMeshByFaces[i] = new BlockMesh();
			Matrix identity = Matrix.Identity;
			identity *= Matrix.CreateRotationY((float)i * (float)Math.PI / 2f) * Matrix.CreateTranslation(0.5f, 0f, 0.5f);
			m_portMeshByFaces[i].AppendModelMeshPart(model.FindMesh("CastingTrough_Port1").MeshParts[0], boneAbsoluteTransform * identity, makeEmissive: false, flipWindingOrder: false, doubleSided: true, flipNormals: false, Color.White);
		}
		m_displayBlockMesh.AppendBlockMesh(m_blockMeshesByData[1]);
		m_displayBlockMesh.TransformPositions(Matrix.CreateTranslation(-0.5f, -0.5f, -0.5f));
	}

	public override bool IsTransparent_(int value)
	{
		return true;
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return 4;
	}

	public override int GetShadowStrength(int value)
	{
		return 0;
	}

	public override Vector3 GetIconBlockOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return new Vector3(0f, 0.1f, 0f);
	}

	public override string GetSoundMaterialName(SubsystemTerrain subsystemTerrain, int value)
	{
		return "Stone";
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 3;
	}

	public FluidTransferrer? CreateFluidTransferrer(SubsystemFluidTransfer subsystemFluidTransfer, SubsystemTerrain subsystemTerrain, int value, Point3 point)
	{
		return new CastingBasinTransferrer(point, subsystemTerrain);
	}

	public FluidConnector? GetConnector(SubsystemTerrain terrain, int value, int face, int x, int y, int z)
	{
		if (face == CellFace.OppositeFace(GetDirection(value)))
		{
			return new FluidConnector(new CellFace(x, y, z, face), FluidConnectorType.Input, 1f);
		}
		return null;
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		generator.GenerateShadedMeshVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], x, y, z, m_blockMeshesByData[GetDirection(value)], Color.White, null, null, geometry.GetGeometry(IndustrialModLoader.IETexture).SubsetAlphaTest);
		int direction = GetDirection(value);
		Point3 point = CellFace.FaceToPoint3(direction);
		int cellValueFast = generator.Terrain.GetCellValueFast(x + point.X, y + point.Y, z + point.Z);
		if (BlockInterfaceResolver.HasFluidConnectorOnFace(generator.Terrain, cellValueFast, CellFace.OppositeFace(direction), x + point.X, y + point.Y, z + point.Z))
		{
			generator.GenerateShadedMeshVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], x, y, z, m_portMeshByFaces[direction], Color.White, null, null, geometry.GetGeometry(IndustrialModLoader.IETexture).SubsetAlphaTest);
		}
		int num = CellFace.OppositeFace(direction);
		Point3 point2 = CellFace.FaceToPoint3(num);
		int cellValueFast2 = generator.Terrain.GetCellValueFast(x + point2.X, y + point2.Y, z + point2.Z);
		if (BlockInterfaceResolver.HasFluidConnectorOnFace(generator.Terrain, cellValueFast2, CellFace.OppositeFace(num), x + point2.X, y + point2.Y, z + point2.Z))
		{
			generator.GenerateShadedMeshVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], x, y, z, m_portMeshByFaces[num], Color.White, null, null, geometry.GetGeometry(IndustrialModLoader.IETexture).SubsetAlphaTest);
		}
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_displayBlockMesh, IndustrialModLoader.IETexture, color, size, ref matrix, environmentData);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new FluidPipeDeviceBehavior(project)
		};
	}

	public Type GetTransferringFluidType()
	{
		return typeof(ITroughTransportableFluid);
	}

	public bool CanConnect(int value, CellFace neighbor, Terrain terrain)
	{
		if (neighbor.Face != GetDirection(value))
		{
			return neighbor.Face == CellFace.OppositeFace(GetDirection(value));
		}
		return true;
	}

	public PipeFluidConnection GetConnection(HashSet<FluidTransferrer> fromTransferrers, HashSet<FluidTransferrer> toTransferrers, HashSet<FluidConnector> fromConnectors, HashSet<FluidConnector> toConnectors, float rate, Type transferringType, SubsystemFluidTransfer subsystemFluidTransfer)
	{
		return new CastingTroughPipeFluidConnection(fromTransferrers, toTransferrers, fromConnectors, toConnectors, rate, transferringType, subsystemFluidTransfer);
	}

	public override int GetMaxStacking(int value)
	{
		return 40;
	}
}
