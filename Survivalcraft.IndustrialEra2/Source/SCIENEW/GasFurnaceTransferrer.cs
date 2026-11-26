using Engine;
using Game;
using SCIENEW.Utils;

namespace SCIENEW;

public class GasFurnaceTransferrer : FluidTransferrer
{
	public GasFurnaceTransferrer(Point3 position, SubsystemTerrain subsystemTerrain)
		: base(position, subsystemTerrain)
	{
	}

	public override ITank GetTank()
	{
		BlockEntityUtils.GetBlockEntity(SubsystemTerrain, Position, out ComponentBlockEntity blockEntity);
		return blockEntity?.Entity.FindComponent<ITank>();
	}

	public override float AcquireFluid(ITank tank, float volume, int fluidValue, CellFace connectorFace)
	{
		return ComponentGasFurnace.AcquireFluid(tank, fluidValue, volume);
	}
}
