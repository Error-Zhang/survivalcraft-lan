using Engine;
using Game;

namespace SCIENEW;

public class CastingCheckValveTransferrer : FluidTransferrer
{
	public CastingCheckValveTransferrer(Point3 position, SubsystemTerrain subsystemTerrain)
		: base(position, subsystemTerrain)
	{
	}

	public override float AcquireFluid(ITank tank, float volume, int fluidValue, CellFace connectorFace)
	{
		return ComponentTankBase.AcquireFluid(tank, fluidValue, volume);
	}

	public override int RemoveFluid(ITank tank, float volume, out float removedVolume, out int removedTankIndex)
	{
		if (tank.GetTankVolume(0) <= 0f)
		{
			return base.RemoveFluid(tank, volume, out removedVolume, out removedTankIndex);
		}
		int tankValue = tank.GetTankValue(0);
		removedVolume = tank.RemoveTankFluid(0, volume);
		removedTankIndex = 0;
		return tankValue;
	}
}
