using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class HydrogenGeneratorDevice : IlluminableDevice, IEntityDevice<ComponentHydrogenGenerator>, IVoltDevice, IFluidTransferrerDevice
{
	public HydrogenGeneratorDevice()
		: base("HydrogenGeneratorDevice", 149, 150, 108, 108, 99, 99)
	{
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 1;
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[3]
		{
			new HydrogenGeneratorDeviceBehavior(project),
			new VoltDeviceBehavior(project),
			new FluidTransferrerDeviceBehavior(project)
		};
	}

	public string GetEntityName(int value)
	{
		return "HydrogenGenerator";
	}

	public Widget GetWidget(IInventory inventory, ComponentHydrogenGenerator component)
	{
		return new HydrogenGeneratorWidget(inventory, component);
	}

	public HashSet<int> GetFaceMask(int value)
	{
		return new HashSet<int> { 0, 1, 2, 3, 4, 5 };
	}

	public VoltElement? GetVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
	{
		return new HydrogenGeneratorVoltElement(subsystemVoltNet, subsystemTerrain, position, blockValue);
	}

	public float GetStandardPL(int value)
	{
		return 4f;
	}

	public float GetStandardUL(int value)
	{
		return 1f;
	}

	public FluidTransferrer? CreateFluidTransferrer(SubsystemFluidTransfer subsystemFluidTransfer, SubsystemTerrain subsystemTerrain, int value, Point3 point)
	{
		return new HydrogenGeneratorTransferrer(point, subsystemTerrain);
	}

	public FluidConnector? GetConnector(Terrain terrain, int value, int face, int x, int y, int z)
	{
		int direction = GetDirection(value);
		DevicesUtils.GetSideFaceOfXZ(direction, out var face2, out var face3);
		int num = CellFace.OppositeFace(direction);
		if (face == num || face == face3 || face == face2)
		{
			return new FluidConnector(new CellFace(x, y, z, face), FluidConnectorType.Input, 1f);
		}
		return null;
	}
}
