using Engine;
using Game;

namespace SCIENEW;

public class ReactorTransferrer : FluidTransferrer
{
	public ReactorTransferrer(Point3 position, SubsystemTerrain subsystemTerrain)
		: base(position, subsystemTerrain)
	{
	}

	public override bool IsFull(ITank tank)
	{
		bool flag = true;
		for (int i = 0; i < 3; i++)
		{
			flag &= tank.GetTankVolume(i) >= tank.GetTankOriginalCapacity(i);
		}
		return flag;
	}

	public override float AcquireFluid(ITank tank, float volume, int fluidValue, CellFace connectorFace)
	{
		return ComponentInventoryTankBase.AcquireFluid(tank, fluidValue, volume, ..3);
	}
}
