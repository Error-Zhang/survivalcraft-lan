using Engine;
using Game;

namespace SCIENEW;

public class CreativeFluidTankTransferrer : FluidTransferrer
{
	public CreativeFluidTankTransferrer(Point3 position, SubsystemTerrain subsystemTerrain)
		: base(position, subsystemTerrain)
	{
	}

	public override int GetFluidValue(ITank tank)
	{
		if (tank is ComponentCreativeFluidTank componentCreativeFluidTank)
		{
			return componentCreativeFluidTank.FluidValue;
		}
		return base.GetFluidValue(tank);
	}

	public override int RemoveFluid(ITank tank, float volume, out float removedVolume, out int removedTankIndex)
	{
		if (tank.GetTankVolume(0) <= 0f)
		{
			return base.RemoveFluid(tank, volume, out removedVolume, out removedTankIndex);
		}
		if (!(tank is ComponentCreativeFluidTank { FluidValue: var fluidValue }))
		{
			return base.RemoveFluid(tank, volume, out removedVolume, out removedTankIndex);
		}
		removedVolume = volume;
		removedTankIndex = 0;
		return fluidValue;
	}

	public override bool IsFull(ITank tank)
	{
		return false;
	}
}
