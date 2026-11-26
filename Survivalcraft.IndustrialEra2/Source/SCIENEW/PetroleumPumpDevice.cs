using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class PetroleumPumpDevice : SixDirectionalDevice, IEntityDevice<ComponentPetroleumPump>, IElectricElementDevice, IFluidTransferrerDevice
{
	public PetroleumPumpDevice()
		: base("PetroleumPumpDevice", 82, 215)
	{
		SideSlot = 81;
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
		result.Value = DevicesBlockManager.GetDeviceValue(Id, DevicesUtils.GetDirectionXYZ(componentMiner, isOpposite: true));
		result.CellFace = raycastResult.CellFace;
		return result;
	}

	public string GetEntityName(int value)
	{
		return "PetroleumPump";
	}

	public Widget GetWidget(IInventory inventory, ComponentPetroleumPump component)
	{
		return new PetroleumPumpWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new PetroleumPumpDeviceBehavior(project),
			new FluidTransferrerDeviceBehavior(project)
		};
	}

	public ElectricElement CreateElectricElement(SubsystemElectricity subsystemElectricity, int value, int x, int y, int z)
	{
		return new PetroleumPumpElectricElement(subsystemElectricity, new Point3(x, y, z));
	}

	public ElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z)
	{
		return ElectricConnectorType.Input;
	}

	public int GetConnectionMask(int value)
	{
		return int.MaxValue;
	}

	public FluidTransferrer? CreateFluidTransferrer(SubsystemFluidTransfer subsystemFluidTransfer, SubsystemTerrain subsystemTerrain, int value, Point3 point)
	{
		return new PetroleumPumpTransferrer(point, subsystemTerrain);
	}

	public FluidConnector? GetConnector(Terrain terrain, int value, int face, int x, int y, int z)
	{
		if (DevicesUtils.GetInnerData(value) != CellFace.OppositeFace(face))
		{
			return null;
		}
		return new FluidConnector(new CellFace(x, y, z, face), FluidConnectorType.Output, 16f);
	}
}
