using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class FractionalPortDevice : DefaultDevice, IFluidTransferrerDevice
{
	public FractionalPortDevice()
		: base("FractionalPortDevice")
	{
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		switch (DevicesUtils.GetInnerData(value))
		{
		case 4:
		case 5:
			generator.GenerateCubeVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], value, x, y, z, Color.White, geometry.GetGeometry(IndustrialModLoader.IETexture).OpaqueSubsetsByFace);
			break;
		case 0:
		case 2:
			generator.GenerateCubeVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], value, x, y, z, 1, 0, 0, Color.White, geometry.GetGeometry(IndustrialModLoader.IETexture).OpaqueSubsetsByFace);
			break;
		default:
			generator.GenerateCubeVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], value, x, y, z, 0, 1, 1, Color.White, geometry.GetGeometry(IndustrialModLoader.IETexture).OpaqueSubsetsByFace);
			break;
		}
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawCubeBlock(primitivesRenderer, DevicesUtils.SetInnerData(value, 4), new Vector3(size), ref matrix, color, color, environmentData, IndustrialModLoader.IETexture);
	}

	public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
	{
		BlockPlacementData result = new BlockPlacementData();
		result.Value = DevicesBlockManager.GetDeviceValue(Id, DevicesUtils.GetDirectionXYZ(componentMiner));
		result.CellFace = raycastResult.CellFace;
		return result;
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		if (face != DevicesUtils.GetInnerData(value) && face != CellFace.OppositeFace(DevicesUtils.GetSixFaceDirection(value)))
		{
			return 233;
		}
		return 215;
	}

	public FluidTransferrer? CreateFluidTransferrer(SubsystemFluidTransfer subsystemFluidTransfer, SubsystemTerrain subsystemTerrain, int value, Point3 point)
	{
		return new FractionalTowerPortTransferrer(point, subsystemTerrain);
	}

	public FluidConnector? GetConnector(Terrain terrain, int value, int face, int x, int y, int z)
	{
		if (face == DevicesUtils.GetInnerData(value))
		{
			return new FluidConnector(new CellFace(x, y, z, face), FluidConnectorType.Output, 16f);
		}
		if (face == CellFace.OppositeFace(DevicesUtils.GetSixFaceDirection(value)))
		{
			return new FluidConnector(new CellFace(x, y, z, face), FluidConnectorType.Output, 16f);
		}
		return null;
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new FluidTransferrerDeviceBehavior(project)
		};
	}
}
