using Engine;
using Game;

namespace SCIENEW;

public class FlaxSeedItem : IEFlatItem
{
	public FlaxSeedItem()
		: base("FlaxSeed", 8)
	{
		Color = new Color(185, 124, 64);
		Category = "Plants";
	}

	public override bool IsPlaceable_(int value)
	{
		return true;
	}

	public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
	{
		BlockPlacementData result = new BlockPlacementData
		{
			CellFace = raycastResult.CellFace
		};
		if (raycastResult.CellFace.Face == 4)
		{
			result.Value = NormalBlockManager.GetNormalBlockID("Flax_Top");
		}
		return result;
	}
}
