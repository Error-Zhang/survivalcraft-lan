using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class CokeOvenPortDevice : SixDirectionalDevice, IEntityDevice<ComponentCokeOvenPort>, IFluidTransferrerDevice
{
	public CokeOvenPortDevice()
		: base("CokeOvenPortDevice", 174, 174, 4)
	{
	}

	public override string GetSoundMaterialName(SubsystemTerrain subsystemTerrain, int value)
	{
		return "Stone";
	}

	public string GetEntityName(int value)
	{
		return "CokeOvenPort";
	}

	public Widget GetWidget(IInventory inventory, ComponentCokeOvenPort component)
	{
		return new CokeOvenPortWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new CokeOvenPortDeviceBehavior(project),
			new FluidTransferrerDeviceBehavior(project)
		};
	}

	public FluidTransferrer? CreateFluidTransferrer(SubsystemFluidTransfer subsystemFluidTransfer, SubsystemTerrain subsystemTerrain, int value, Point3 point)
	{
		return new CokeOvenPortTransferrer(point, subsystemTerrain);
	}

	public FluidConnector? GetConnector(Terrain terrain, int value, int face, int x, int y, int z)
	{
		if (face == DevicesUtils.GetInnerData(value))
		{
			return new FluidConnector(new CellFace(x, y, z, face), FluidConnectorType.Output, 16f);
		}
		return null;
	}
}
