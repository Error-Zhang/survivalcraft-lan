using RecipaediaEX.Events;
using SCIENEW.Events;

namespace SCIENEW;

public class FluidConnection(FluidTransferrer fromTransferrer, FluidTransferrer toTransferrer, FluidConnector fromConnector, FluidConnector toConnector, float rate)
{
	public FluidTransferrer FromTransferrer = fromTransferrer;

	public FluidTransferrer ToTransferrer = toTransferrer;

	public FluidConnector FromConnector = fromConnector;

	public FluidConnector ToConnector = toConnector;

	public float Rate = rate;

	public void Simulate(float dt)
	{
		ITank tank = ToTransferrer.GetTank();
		if (tank == null || ToTransferrer.IsFull(tank))
		{
			return;
		}
		ITank tank2 = FromTransferrer.GetTank();
		if (tank2 == null)
		{
			return;
		}
		float volume = Rate * dt;
		int fluidValue = FromTransferrer.GetFluidValue(tank2);
		if (fluidValue == 0)
		{
			return;
		}
		FluidPushingContext fluidPushingContext = new FluidPushingContext(FromTransferrer.Position, ToTransferrer.Position, fluidValue, volume);
		if (RecipaediaInterceptBus.TryProceed(fluidPushingContext))
		{
			volume = fluidPushingContext.Volume;
			if (!(volume <= 0f))
			{
				fluidValue = FromTransferrer.RemoveFluid(tank2, volume, out var removedVolume, out var removedTankIndex);
				float volume2 = ToTransferrer.AcquireFluid(tank, removedVolume, fluidValue, ToConnector.MountedFace);
				tank2.AddTankFluid(removedTankIndex, fluidValue, volume2, out var _);
			}
		}
	}
}
