using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class LiquidPumpDevice : SixDirectionalDevice, IEntityDevice<ComponentLiquidPump>, IElectricElementDevice, IFluidTransferrerDevice
{
	public LiquidPumpDevice()
		: base("LiquidPumpDevice", 203, 204)
	{
	}

	public string GetEntityName(int value)
	{
		return "LiquidPump";
	}

	public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
	{
		BlockPlacementData result = new BlockPlacementData();
		result.Value = DevicesBlockManager.GetDeviceValue(Id, DevicesUtils.GetDirectionXYZ(componentMiner, isOpposite: true));
		result.CellFace = raycastResult.CellFace;
		return result;
	}

	public Widget GetWidget(IInventory inventory, ComponentLiquidPump component)
	{
		return new LiquidPumpWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new LiquidPumpDeviceBehavior(project),
			new FluidTransferrerDeviceBehavior(project)
		};
	}

	public ElectricElement CreateElectricElement(SubsystemElectricity subsystemElectricity, int value, int x, int y, int z)
	{
		return new LiquidPumpElectricElement(subsystemElectricity, new Point3(x, y, z));
	}

	public ElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z)
	{
		return ElectricConnectorType.Input;
	}

	public int GetConnectionMask(int value)
	{
		return int.MaxValue;
	}

	public FluidConnector? GetConnector(Terrain terrain, int value, int face, int x, int y, int z)
	{
		if (DevicesUtils.GetInnerData(value) != CellFace.OppositeFace(face))
		{
			return null;
		}
		return new FluidConnector(new CellFace(x, y, z, face), FluidConnectorType.Output, 16f);
	}

	public FluidTransferrer? CreateFluidTransferrer(SubsystemFluidTransfer subsystemFluidTransfer, SubsystemTerrain subsystemTerrain, int value, Point3 point)
	{
		return new LiquidPumpTransferrer(point, subsystemTerrain);
	}
}
