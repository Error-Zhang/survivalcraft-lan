using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class HearthFurnaceOutputPortDevice : FourDirectionalDevice, IFluidTransferrerDevice
{
	public HearthFurnaceOutputPortDevice()
		: base("HearthFurnaceOutputPortDevice", 230, 230, 4, 4, 4)
	{
	}

	public override string GetSoundMaterialName(SubsystemTerrain subsystemTerrain, int value)
	{
		return "Stone";
	}

	public FluidTransferrer? CreateFluidTransferrer(SubsystemFluidTransfer subsystemFluidTransfer, SubsystemTerrain subsystemTerrain, int value, Point3 point)
	{
		return new HearthFurnaceOutputPortTransferrer(point, subsystemTerrain);
	}

	public FluidConnector? GetConnector(Terrain terrain, int value, int face, int x, int y, int z)
	{
		if (face == GetDirection(value))
		{
			return new FluidConnector(new CellFace(x, y, z, face), FluidConnectorType.Output, 1f);
		}
		if (face == CellFace.OppositeFace(GetDirection(value)))
		{
			return new FluidConnector(new CellFace(x, y, z, face), FluidConnectorType.Output, 1f);
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
