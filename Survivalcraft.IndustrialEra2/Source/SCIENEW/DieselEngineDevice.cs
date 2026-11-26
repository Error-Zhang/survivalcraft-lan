using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class DieselEngineDevice : IlluminableDevice, IEntityDevice<ComponentDieselEngine>, IFluidTransferrerDevice
{
	public DieselEngineDevice()
		: base("DieselEngineDevice", 89, 91, 90)
	{
		DownSlot = 204;
		SideSlot = 109;
	}

	public string GetEntityName(int value)
	{
		return "DieselEngine";
	}

	public Widget GetWidget(IInventory inventory, ComponentDieselEngine component)
	{
		return new DieselEngineWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new DieselEngineDeviceBehavior(project),
			new FluidTransferrerDeviceBehavior(project)
		};
	}

	public FluidTransferrer? CreateFluidTransferrer(SubsystemFluidTransfer subsystemFluidTransfer, SubsystemTerrain subsystemTerrain, int value, Point3 point)
	{
		return new DieselEngineTransferrer(point, subsystemTerrain);
	}

	public FluidConnector? GetConnector(Terrain terrain, int value, int face, int x, int y, int z)
	{
		int direction = GetDirection(value);
		DevicesUtils.GetSideFaceOfXZ(direction, out var face2, out var face3);
		int num = CellFace.OppositeFace(direction);
		if (face == num || face == face3 || face == face2 || face == 5)
		{
			return new FluidConnector(new CellFace(x, y, z, face), FluidConnectorType.Input, 16f);
		}
		return null;
	}
}
