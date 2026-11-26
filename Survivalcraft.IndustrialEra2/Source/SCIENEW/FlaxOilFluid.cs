using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class FlaxOilFluid : Fluid, IPlaceableFluid, IScoopableFluid, IFuelFluid, IPipeableFluid
{
	public override string GetCraftingID(int value)
	{
		return "FlaxOilFluid";
	}

	public bool IsScoopable(int container, out int bucketIndex)
	{
		bucketIndex = 0;
		if (container == ItemsBlockManager.GetItemID("GlassBottle"))
		{
			bucketIndex = ItemsBlockManager.GetItemID("Bottle.FlaxOil");
			return true;
		}
		if (container == EmptyBucketBlock.Index)
		{
			bucketIndex = ItemsBlockManager.GetItemID("FlaxOilBucket");
			return true;
		}
		return false;
	}

	public BiDictionary<int, int> AcquireContainer(int value)
	{
		return new BiDictionary<int, int>
		{
			{
				ItemsBlockManager.GetItemID("GlassBottle"),
				ItemsBlockManager.GetItemID("Bottle.FlaxOil")
			},
			{
				EmptyBucketBlock.Index,
				ItemsBlockManager.GetItemID("FlaxOilBucket")
			}
		};
	}

	public override Color GetColor(int value)
	{
		return new Color(255, 200, 0);
	}

	public override int GetCongenericFluidCount()
	{
		return 1;
	}

	public int GetFluidBlockIndex(int value)
	{
		return FlaxOilBlock.Index;
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
		return 2f / 15f;
	}
}
