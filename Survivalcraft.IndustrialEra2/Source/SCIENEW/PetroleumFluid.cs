using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class PetroleumFluid : Fluid, IScoopableFluid, IPlaceableFluid, IPipeableFluid, IFuelFluid
{
	public BiDictionary<int, int> AcquireContainer(int value)
	{
		return new BiDictionary<int, int>
		{
			{
				EmptyBucketBlock.Index,
				ItemsBlockManager.GetItemID("PetroleumBucket")
			},
			{
				OilTankDevice.SetEmpty(DevicesBlockManager.GetDeviceValue("OilTankDevice", 0)),
				OilTankDevice.SetFull(DevicesBlockManager.GetDeviceValue("OilTankDevice", 0))
			}
		};
	}

	public override string GetCraftingID(int value)
	{
		return "PetroleumFluid";
	}

	public bool IsScoopable(int container, out int bucketIndex)
	{
		bucketIndex = 0;
		if (container == EmptyBucketBlock.Index)
		{
			bucketIndex = ItemsBlockManager.GetItemID("PetroleumBucket");
			return true;
		}
		return false;
	}

	public override Color GetColor(int value)
	{
		return Color.DarkGray;
	}

	public override int GetCongenericFluidCount()
	{
		return 1;
	}

	public int GetFluidBlockIndex(int value)
	{
		return PetroleumBlock.Index;
	}

	public override void Emit(Project project, Vector3 position, Vector3 velocity)
	{
	}

	public float GetFuelLevel(int value)
	{
		return 1f;
	}

	public float GetFuelSpeed(int value)
	{
		return 1f / 15f;
	}
}
