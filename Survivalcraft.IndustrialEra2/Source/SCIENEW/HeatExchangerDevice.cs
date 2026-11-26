using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class HeatExchangerDevice : FourDirectionalDevice, IEntityDevice<ComponentHeatExchanger>, IFluidTransferrerDevice
{
	public HeatExchangerDevice()
		: base("HeatExchangerDevice", 121, 139, 140, 141, 141)
	{
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 2;
	}

	public string GetEntityName(int value)
	{
		return "HeatExchanger";
	}

	public Widget GetWidget(IInventory inventory, ComponentHeatExchanger component)
	{
		return new HeatExchangerWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new HeatExchangerDeviceBehavior(project),
			new FluidTransferrerDeviceBehavior(project)
		};
	}

	public FluidTransferrer? CreateFluidTransferrer(SubsystemFluidTransfer subsystemFluidTransfer, SubsystemTerrain subsystemTerrain, int value, Point3 point)
	{
		return new HeatExchangerTransferrer(point, subsystemTerrain);
	}

	public FluidConnector? GetConnector(Terrain terrain, int value, int face, int x, int y, int z)
	{
		if (face == 4 || face == 5)
		{
			return null;
		}
		return new FluidConnector(new CellFace(x, y, z, face), FluidConnectorType.Input, 16f);
	}
}
