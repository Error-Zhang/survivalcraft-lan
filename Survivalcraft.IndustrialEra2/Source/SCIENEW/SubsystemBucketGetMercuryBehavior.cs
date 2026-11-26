using Engine;
using Game;

namespace SCIENEW;

public class SubsystemBucketGetMercuryBehavior : SubsystemBlockBehavior
{
	public override int[] HandledBlocks => new int[1] { BlocksManager.GetBlockIndex<EmptyBucketBlock>() };

	public override bool OnUse(Ray3 ray, ComponentMiner componentMiner)
	{
		object obj = componentMiner.Raycast(ray, RaycastMode.Gathering);
		if (obj is TerrainRaycastResult)
		{
			CellFace cellFace = ((TerrainRaycastResult)obj).CellFace;
			int cellValue = SubsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z);
			int num = Terrain.ExtractContents(cellValue);
			int data = Terrain.ExtractData(cellValue);
			if (BlocksManager.Blocks[num] is MercuryBlock && FocalorsFluidBlock.GetLevel(data) <= 0.1f)
			{
				return IndustrialFluidBucket.Fill(SubsystemTerrain, componentMiner, cellFace, ItemsBlockManager.GetItemID("MercuryBucket"));
			}
		}
		return false;
	}
}
