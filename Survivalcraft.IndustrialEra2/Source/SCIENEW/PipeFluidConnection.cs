using System;
using System.Collections.Generic;
using System.Linq;
using Engine;
using RecipaediaEX.Events;
using SCIENEW.Events;
using ZLinq;

namespace SCIENEW;

public class PipeFluidConnection(HashSet<FluidTransferrer> fromTransferrers, HashSet<FluidTransferrer> toTransferrers, HashSet<FluidConnector> fromConnectors, HashSet<FluidConnector> toConnectors, float rate, Type transferringType, SubsystemFluidTransfer subsystemFluidTransfer)
{
	public HashSet<FluidTransferrer> FromTransferrers = fromTransferrers;

	public HashSet<FluidTransferrer> ToTransferrers = toTransferrers;

	public HashSet<FluidConnector> FromConnectors = fromConnectors;

	public HashSet<FluidConnector> ToConnectors = toConnectors;

	public float Rate = rate;

	public Type TransferringType = transferringType;

	public SubsystemFluidTransfer SubsystemFluidTransfer = subsystemFluidTransfer;

	public bool IsTransferring { get; protected set; }

	public int? TransferringFluid { get; protected set; }

	public virtual void UpdateRate()
	{
		foreach (FluidConnector item in fromConnectors)
		{
			Rate = MathF.Min(Rate, item.Rate);
		}
		foreach (FluidConnector item2 in toConnectors)
		{
			Rate = MathF.Min(Rate, item2.Rate);
		}
	}

	public virtual Vector3 CalcAveragePosition()
	{
		Vector3 zero = Vector3.Zero;
		foreach (FluidTransferrer item in fromTransferrers)
		{
			zero += new Vector3(item.Position);
		}
		foreach (FluidTransferrer item2 in toTransferrers)
		{
			zero += new Vector3(item2.Position);
		}
		zero /= (float)(fromTransferrers.Count + toTransferrers.Count);
		return zero + new Vector3(0.5f);
	}

	public virtual void Init()
	{
	}

	public virtual void Simulate(float dt)
	{
		IsTransferring = false;
		if (toTransferrers.AsValueEnumerable().Any((FluidTransferrer x) => x.GetTank() == null) || fromTransferrers.AsValueEnumerable().Any((FluidTransferrer x) => x.GetTank() == null) || toTransferrers.AsValueEnumerable().All((FluidTransferrer x) => x.IsFull(x.GetTank())))
		{
			return;
		}
		float volume = Rate * dt;
		TransferringFluid = null;
		foreach (FluidTransferrer item in fromTransferrers)
		{
			if (TransferringFluid.HasValue)
			{
				break;
			}
			int fluidValue = item.GetFluidValue(item.GetTank());
			if (FluidsManager.GetFluid(fluidValue).GetType().GetInterfaces()
				.Contains(transferringType, null))
			{
				TransferringFluid = fluidValue;
			}
		}
		if (!TransferringFluid.HasValue)
		{
			return;
		}
		FluidTransferrer fluidTransferrer = fromTransferrers.AsValueEnumerable().FirstOrDefault();
		FluidTransferrer fluidTransferrer2 = toTransferrers.AsValueEnumerable().FirstOrDefault();
		if (fluidTransferrer == null || fluidTransferrer2 == null)
		{
			return;
		}
		FluidPushingContext fluidPushingContext = new FluidPushingContext(fluidTransferrer.Position, fluidTransferrer2.Position, TransferringFluid.Value, volume);
		if (!RecipaediaInterceptBus.TryProceed(fluidPushingContext))
		{
			return;
		}
		volume = fluidPushingContext.Volume;
		if (volume <= 0f)
		{
			return;
		}
		float num = 0f;
		int num2 = 0;
		foreach (FluidTransferrer item2 in fromTransferrers)
		{
			ITank tank = item2.GetTank();
			if (item2.GetFluidValue(tank) == TransferringFluid)
			{
				item2.RemoveFluid(tank, volume, out var removedVolume, out var _);
				num += removedVolume;
				num2++;
			}
		}
		IsTransferring = num > 0f;
		int num3 = 0;
		foreach (FluidTransferrer item3 in toTransferrers)
		{
			if (num <= 0f)
			{
				break;
			}
			ITank tank2 = item3.GetTank();
			float num4 = num / ((float)toTransferrers.Count / (float)num2);
			float num5 = MathF.Max(0f, item3.AcquireFluid(tank2, num4, TransferringFluid.Value, toConnectors.ElementAt(num3++).MountedFace));
			float num6 = num4 - num5;
			num -= num6;
		}
		num3 = 0;
		foreach (FluidTransferrer item4 in fromTransferrers)
		{
			if (num <= 0f)
			{
				break;
			}
			ITank tank3 = item4.GetTank();
			num = item4.AcquireFluid(tank3, num, TransferringFluid.Value, fromConnectors.ElementAt(num3++).MountedFace);
		}
	}
}
