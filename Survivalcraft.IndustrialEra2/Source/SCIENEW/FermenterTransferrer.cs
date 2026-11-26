using Engine;
using Game;

namespace SCIENEW;

public class FermenterTransferrer : FluidTransferrer
{
	public FermenterTransferrer(Point3 position, SubsystemTerrain subsystemTerrain)
		: base(position, subsystemTerrain)
	{
	}

	public override int GetFluidValue(ITank tank)
	{
		return tank.GetTankValue(0);
	}

	public override int RemoveFluid(ITank tank, float volume, out float removedVolume, out int removedTankIndex)
	{
		if (tank.GetTankVolume(0) <= 0f)
		{
			return base.RemoveFluid(tank, volume, out removedVolume, out removedTankIndex);
		}
		if (!(tank is ComponentFermenter))
		{
			return base.RemoveFluid(tank, volume, out removedVolume, out removedTankIndex);
		}
		int tankValue = tank.GetTankValue(0);
		removedVolume = tank.RemoveTankFluid(0, volume);
		removedTankIndex = 0;
		return tankValue;
	}
}
