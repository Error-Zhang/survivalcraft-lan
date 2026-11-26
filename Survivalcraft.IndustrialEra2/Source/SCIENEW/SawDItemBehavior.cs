using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class SawDItemBehavior : DamageableItemBehavior
{
	public SawDItemBehavior(Project project)
		: base(project)
	{
	}

	public override void OnMinerDig(ComponentMiner miner, TerrainRaycastResult raycastResult, ref float DigProgress, out bool Digged)
	{
		base.OnMinerDig(miner, raycastResult, ref DigProgress, out Digged);
		int activeBlockValue = miner.ActiveBlockValue;
		if (!ElectricToolDItem.IsOutOfPower(activeBlockValue))
		{
			CellFace cellFace = raycastResult.CellFace;
			int cellValue = miner.m_subsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z);
			if (!(BlocksManager.Blocks[Terrain.ExtractContents(cellValue)].GetCategory(cellValue) == "Plants") || !(DigProgress >= 1f))
			{
				return;
			}
			miner.DigCellFace = null;
			miner.Poke(forceRestart: true);
			int num = 0;
			for (int i = -2; i <= 2; i++)
			{
				for (int j = -2; j <= 2; j++)
				{
					for (int k = 0; k < 25; k++)
					{
						Point3 point = raycastResult.CellFace.Point + new Point3(i, k, j);
						int cellValue2 = miner.m_subsystemTerrain.Terrain.GetCellValue(point.X, point.Y, point.Z);
						Block block = BlocksManager.Blocks[Terrain.ExtractContents(cellValue2)];
						if (!(block.GetCategory(cellValue2) != "Plants"))
						{
							BlockPlacementData digValue = block.GetDigValue(miner.m_subsystemTerrain, miner, cellValue2, activeBlockValue, raycastResult);
							miner.m_subsystemTerrain.DestroyCell(BlocksManager.Blocks[SteelAxeBlock.Index].GetToolLevel(SteelAxeBlock.Index), point.X, point.Y, point.Z, digValue.Value, noDrop: false, noParticleSystem: false);
							if (new Point3(i, k, j) == Point3.Zero)
							{
								miner.m_subsystemSoundMaterials.PlayImpactSound(cellValue, new Vector3(cellFace.X, cellFace.Y, cellFace.Z), 2f);
							}
							num++;
						}
					}
				}
			}
			Digged = false;
			DamageableItemManager.DamageActiveTool(miner, num);
		}
		else
		{
			DigProgress = 0f;
		}
	}
}
