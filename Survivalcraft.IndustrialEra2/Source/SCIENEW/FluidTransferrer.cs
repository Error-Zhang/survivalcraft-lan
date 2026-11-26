using Engine;
using Game;
using SCIENEW.Utils;

namespace SCIENEW;

public abstract class FluidTransferrer
{
	public Point3 Position;

	public SubsystemTerrain SubsystemTerrain;

	public ITank Tank;

	public FluidTransferrer(Point3 position, SubsystemTerrain subsystemTerrain)
	{
		Position = position;
		SubsystemTerrain = subsystemTerrain;
	}

	public virtual ITank GetTank()
	{
		if (BlockEntityUtils.GetBlockEntity(SubsystemTerrain, Position, out ComponentBlockEntity blockEntity))
		{
			ComponentTankBase componentTankBase = blockEntity.Entity.FindComponent<ComponentTankBase>();
			ComponentInventoryTankBase result = blockEntity.Entity.FindComponent<ComponentInventoryTankBase>();
			if (componentTankBase != null)
			{
				return componentTankBase;
			}
			return result;
		}
		return null;
	}

	public virtual bool IsFull(ITank tank)
	{
		return tank.GetTankVolume(0) >= tank.GetTankOriginalCapacity(0);
	}

	public virtual int RemoveFluid(ITank tank, float volume, out float removedVolume, out int removedTankIndex)
	{
		removedVolume = 0f;
		removedTankIndex = 0;
		return 0;
	}

	public virtual int GetFluidValue(ITank tank)
	{
		return tank.GetTankValue(0);
	}

	public virtual float AcquireFluid(ITank tank, float volume, int fluidValue, CellFace connectorFace)
	{
		return ComponentTankBase.AcquireFluid(tank, fluidValue, volume);
	}

	public virtual void OnAdded()
	{
	}

	public virtual void OnRemoved()
	{
	}

	public virtual void Simulate(float dt)
	{
	}
}
