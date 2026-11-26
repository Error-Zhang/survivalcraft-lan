using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class CombustionEngineDevice : IlluminableDevice, IEntityDevice<ComponentCombustionEngine>, IFluidTransferrerDevice
{
	public CombustionEngineDevice()
		: base("CombustionEngineDevice", 89, 91, 90)
	{
		DownSlot = 204;
		SideSlot = 88;
	}

	public string GetEntityName(int value)
	{
		return "CombustionEngine";
	}

	public Widget GetWidget(IInventory inventory, ComponentCombustionEngine component)
	{
		return new CombustionEngineWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new CombustionEngineDeviceBehavior(project),
			new FluidTransferrerDeviceBehavior(project)
		};
	}

	public FluidTransferrer? CreateFluidTransferrer(SubsystemFluidTransfer subsystemFluidTransfer, SubsystemTerrain subsystemTerrain, int value, Point3 point)
	{
		return new CombustionEngineTransferrer(point, subsystemTerrain);
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
