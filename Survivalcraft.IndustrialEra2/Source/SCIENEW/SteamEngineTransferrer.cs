using Engine;
using Game;

namespace SCIENEW;

public class SteamEngineTransferrer : FluidTransferrer
{
	public SteamEngineTransferrer(Point3 position, SubsystemTerrain subsystemTerrain)
		: base(position, subsystemTerrain)
	{
	}

	public override float AcquireFluid(ITank tank, float volume, int fluidValue, CellFace connectorFace)
	{
		return ComponentInventoryTankBase.AcquireFluid(tank, fluidValue, volume);
	}
}
