using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class SteamEngineDevice : IlluminableDevice, IEntityDevice<ComponentSteamEngine>, IFluidTransferrerDevice
{
	public SteamEngineDevice()
		: base("SteamEngineDevice", 143, 175, 204)
	{
		DownSlot = 204;
	}

	public FluidTransferrer? CreateFluidTransferrer(SubsystemFluidTransfer subsystemFluidTransfer, SubsystemTerrain subsystemTerrain, int value, Point3 point)
	{
		return new SteamEngineTransferrer(point, subsystemTerrain);
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

	public string GetEntityName(int value)
	{
		return "SteamEngine";
	}

	public Widget GetWidget(IInventory inventory, ComponentSteamEngine component)
	{
		return new SteamEngineWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new SteamEngineDeviceBehavior(project),
			new FluidTransferrerDeviceBehavior(project)
		};
	}
}
