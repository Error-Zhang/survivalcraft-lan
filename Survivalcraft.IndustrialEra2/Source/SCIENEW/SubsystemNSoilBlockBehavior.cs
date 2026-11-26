using Game;

namespace SCIENEW;

public class SubsystemNSoilBlockBehavior : SubsystemSoilBlockBehavior, IUpdateable
{
	public override void OnHitByProjectile(CellFace cellFace, WorldItem worldItem)
	{
		if (cellFace.Y < 0 || cellFace.Y > 255 || worldItem.Velocity.Length() < 10f || cellFace.Face == 5 || SubsystemTerrain.Terrain.GetCellContents(cellFace.X, cellFace.Y + 1, cellFace.Z) != AirBlock.Index)
		{
			return;
		}
		int num = Terrain.ExtractContents(worldItem.Value);
		if (num == 102)
		{
			for (int i = -1; i < 2; i++)
			{
				for (int j = -1; j < 2; j++)
				{
					int cellValueFast = SubsystemTerrain.Terrain.GetCellValueFast(cellFace.X + i, cellFace.Y, cellFace.Z + j);
					if (Terrain.ExtractContents(cellValueFast) == 168)
					{
						SubsystemTerrain.ChangeCell(cellFace.X + i, cellFace.Y, cellFace.Z + j, 0xA8 | (SoilBlock.SetNitrogen(Terrain.ExtractData(cellValueFast), 3) << 14));
						if (worldItem is Pickable { Count: >1 } pickable)
						{
							pickable.Count--;
						}
						else
						{
							worldItem.ToRemove = true;
						}
					}
				}
			}
		}
		else
		{
			if ((num != SeedsBlock.Index && Terrain.ExtractContents(num) != BaseItemBlock.Index) || SubsystemTerrain.Terrain.GetCellContentsFast(cellFace.X, cellFace.Y + 1, cellFace.Z) != 0)
			{
				return;
			}
			num = 0;
			int value = worldItem.Value;
			switch (value)
			{
			case 16557:
				num = 0x14 | (FlowerBlock.SetIsSmall(0, isSmall: true) << 14);
				break;
			case 173:
				num = 0x13 | (TallGrassBlock.SetIsSmall(0, isSmall: true) << 14);
				break;
			case 49325:
				num = 0x19 | (FlowerBlock.SetIsSmall(0, isSmall: true) << 14);
				break;
			case 32941:
				num = 0x18 | (FlowerBlock.SetIsSmall(0, isSmall: true) << 14);
				break;
			case 82093:
				num = 0xAE | (RyeBlock.SetSize(RyeBlock.SetIsWild(0, isWild: false), 0) << 14);
				break;
			case 65709:
				num = 0xAE | (RyeBlock.SetSize(RyeBlock.SetIsWild(0, isWild: false), 0) << 14);
				break;
			case 114861:
				num = 0x83 | (BasePumpkinBlock.SetSize(BasePumpkinBlock.SetIsDead(0, isDead: false), 0) << 14);
				break;
			case 98477:
				num = 0xCC | (CottonBlock.SetSize(CottonBlock.SetIsWild(0, isWild: false), 0) << 14);
				break;
			default:
				if (value == ItemsBlockManager.GetItemID("FlaxSeed"))
				{
					num = NormalBlockManager.GetNormalBlockID("Flax_Top");
				}
				break;
			}
			if (num != 0)
			{
				SubsystemTerrain.ChangeCell(cellFace.X, cellFace.Y + 1, cellFace.Z, num);
				if (worldItem is Pickable { Count: >1 } pickable2)
				{
					pickable2.Count--;
				}
				else
				{
					worldItem.ToRemove = true;
				}
			}
		}
	}
}
