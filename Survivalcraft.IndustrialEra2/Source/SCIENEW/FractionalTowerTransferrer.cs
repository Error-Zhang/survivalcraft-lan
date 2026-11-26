using Engine;
using Game;

namespace SCIENEW;

public class FractionalTowerTransferrer : FluidTransferrer
{
	public FractionalTowerTransferrer(Point3 position, SubsystemTerrain subsystemTerrain)
		: base(position, subsystemTerrain)
	{
	}

	public override float AcquireFluid(ITank tank, float volume, int fluidValue, CellFace connectorFace)
	{
		return ComponentTankBase.AcquireFluid(tank, fluidValue, volume, 0..1);
	}
}
