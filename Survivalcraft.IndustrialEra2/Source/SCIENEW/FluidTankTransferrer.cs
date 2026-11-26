using Engine;
using Game;
using SCIENEW.Utils;

namespace SCIENEW;

public class FluidTankTransferrer : FluidTransferrer
{
	public FluidTankTransferrer(Point3 position, SubsystemTerrain subsystemTerrain)
		: base(position, subsystemTerrain)
	{
	}

	public override ITank GetTank()
	{
		Point3 position = Position;
		ComponentBlockEntity blockEntity;
		while (!BlockEntityUtils.GetBlockEntity(SubsystemTerrain.Project.FindSubsystem<SubsystemTerrain>(throwOnError: true), position, out blockEntity) || blockEntity.Entity.FindComponent<ComponentFluidTank>() == null)
		{
			position -= Point3.UnitY;
			if (position.Y <= 0)
			{
				return null;
			}
		}
		return blockEntity.Entity.FindComponent<ComponentFluidTank>(throwOnError: true);
	}

	public override int GetFluidValue(ITank tank)
	{
		if (tank is ComponentFluidTank { IsCapacityResolved: false })
		{
			return 0;
		}
		return base.GetFluidValue(tank);
	}

	public override int RemoveFluid(ITank tank, float volume, out float removedVolume, out int removedTankIndex)
	{
		if (tank is ComponentFluidTank { IsCapacityResolved: false })
		{
			return base.RemoveFluid(tank, volume, out removedVolume, out removedTankIndex);
		}
		if (tank.GetTankVolume(0) <= 0f)
		{
			return base.RemoveFluid(tank, volume, out removedVolume, out removedTankIndex);
		}
		int tankValue = tank.GetTankValue(0);
		removedVolume = tank.RemoveTankFluid(0, volume);
		removedTankIndex = 0;
		return tankValue;
	}

	public override bool IsFull(ITank tank)
	{
		if (tank is ComponentFluidTank { IsCapacityResolved: false })
		{
			return true;
		}
		return tank.GetTankVolume(0) >= tank.GetTankOriginalCapacity(0);
	}
}
