using Engine;
using Game;

namespace SCIENEW;

public class LiquidPumpTransferrer : FluidTransferrer
{
	public LiquidPumpTransferrer(Point3 position, SubsystemTerrain subsystemTerrain)
		: base(position, subsystemTerrain)
	{
	}

	public override int RemoveFluid(ITank tank, float volume, out float removedVolume, out int removedTankIndex)
	{
		removedVolume = 0f;
		removedTankIndex = 0;
		ComponentLiquidPump componentLiquidPump = tank as ComponentLiquidPump;
		for (int i = 0; i < componentLiquidPump.SlotsCount; i++)
		{
			if (!(componentLiquidPump.GetTankVolume(i) <= 0f))
			{
				removedTankIndex = i;
				int tankValue = componentLiquidPump.GetTankValue(i);
				removedVolume = componentLiquidPump.RemoveTankFluid(i, volume);
				return tankValue;
			}
		}
		return 0;
	}

	public override int GetFluidValue(ITank tank)
	{
		ComponentLiquidPump componentLiquidPump = tank as ComponentLiquidPump;
		for (int i = 0; i < componentLiquidPump.SlotsCount; i++)
		{
			if (!(componentLiquidPump.GetTankVolume(i) <= 0f))
			{
				return componentLiquidPump.GetTankValue(i);
			}
		}
		return 0;
	}
}
