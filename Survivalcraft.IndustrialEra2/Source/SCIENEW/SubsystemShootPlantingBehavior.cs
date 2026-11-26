using System.Collections.Generic;
using Game;

namespace SCIENEW;

public class SubsystemShootPlantingBehavior : SubsystemBlockBehavior
{
	public readonly Dictionary<int, HashSet<int>> SeedDirtMap = new Dictionary<int, HashSet<int>>
	{
		{
			CactusBlock.Index,
			new HashSet<int> { SandBlock.Index }
		},
		{
			ItemsBlockManager.GetItemID("FlaxSeed"),
			new HashSet<int>
			{
				GrassBlock.Index,
				DirtBlock.Index
			}
		}
	};

	public readonly Dictionary<int, int> SeedPlantMap = new Dictionary<int, int>
	{
		{
			CactusBlock.Index,
			CactusBlock.Index
		},
		{
			ItemsBlockManager.GetItemID("FlaxSeed"),
			NormalBlockManager.GetNormalBlockID("Flax_Top")
		}
	};

	public override int[] HandledBlocks => new int[3]
	{
		BlocksManager.GetBlockIndex<DirtBlock>(),
		BlocksManager.GetBlockIndex<GrassBlock>(),
		BlocksManager.GetBlockIndex<SandBlock>()
	};

	public override void OnHitByProjectile(CellFace cellFace, WorldItem worldItem)
	{
		if (cellFace.Y >= 0 && cellFace.Y <= 255 && !(worldItem.Velocity.Length() < 10f))
		{
			int key = Terrain.ReplaceLight(worldItem.Value, 0);
			int cellContents = SubsystemTerrain.Terrain.GetCellContents(cellFace.X, cellFace.Y, cellFace.Z);
			if (SeedDirtMap.TryGetValue(key, out HashSet<int> value) && value.Contains(cellContents))
			{
				int value2 = SeedPlantMap[key];
				SubsystemTerrain.ChangeCell(cellFace.X, cellFace.Y + 1, cellFace.Z, value2);
				worldItem.ToRemove = true;
			}
		}
	}
}
