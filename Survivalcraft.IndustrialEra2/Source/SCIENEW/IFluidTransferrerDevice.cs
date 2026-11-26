using Engine;
using Game;

namespace SCIENEW;

public interface IFluidTransferrerDevice
{
	FluidTransferrer? CreateFluidTransferrer(SubsystemFluidTransfer subsystemFluidTransfer, SubsystemTerrain subsystemTerrain, int value, Point3 point);

	FluidConnector? GetConnector(Terrain terrain, int value, int face, int x, int y, int z);
}
