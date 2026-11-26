using System.Collections.Generic;
using System.Linq;
using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class MiningDrillDItemBehavior : DamageableItemBehavior
{
	public delegate Point3[] GetPointsFunc(CellFace cellFace);

	public GetPointsFunc PointsGetter;

	public List<string> m_drillsToSwitch;

	public MiningDrillDItemBehavior(Project project, GetPointsFunc pointsGetter)
		: base(project)
	{
		PointsGetter = pointsGetter;
		m_drillsToSwitch = (from x in DamageableItemManager.GetItems<MiningDrillDItem>()
			select x.GetCraftingId()).ToList();
	}

	public override bool OnUse(Ray3 ray, ComponentMiner componentMiner)
	{
		if (Terrain.ExtractContents(componentMiner.ActiveBlockValue) != BlocksManager.GetBlockIndex<BaseDamageableItemBlock>())
		{
			return false;
		}
		if (!(DamageableItemManager.GetItem(componentMiner.ActiveBlockValue) is MiningDrillDItem miningDrillDItem))
		{
			return false;
		}
		int damage = miningDrillDItem.GetDamage(componentMiner.ActiveBlockValue);
		int num = m_drillsToSwitch.IndexOf(miningDrillDItem.GetCraftingId());
		string text = m_drillsToSwitch[(num + 1) % m_drillsToSwitch.Count];
		if (!IndustrialEraLanAdapter.IsRemote(Project))
		{
			componentMiner.Inventory.RemoveSlotItems(componentMiner.Inventory.ActiveSlotIndex, 1);
			int value = DamageableItemManager.SetDamage(DamageableItemManager.GetItemID(text), damage);
			componentMiner.Inventory.AddSlotItems(componentMiner.Inventory.ActiveSlotIndex, value, 1);
		}
		string arg = LanguageControl.Get("Behaviors", "MiningDrillDItemBehavior", ((MiningDrillDItem)DamageableItemManager.GetItem(text)).m_suffix);
		componentMiner.ComponentPlayer.ComponentGui.DisplaySmallMessage(string.Format(LanguageControl.Get("Behaviors", "MiningDrillDItemBehavior", "SwitchTip"), arg), Color.White, blinking: false, playNotificationSound: false);
		return true;
	}

	public override void OnMinerDig(ComponentMiner miner, TerrainRaycastResult raycastResult, ref float DigProgress, out bool Digged)
	{
		base.OnMinerDig(miner, raycastResult, ref DigProgress, out Digged);
		int activeBlockValue = miner.ActiveBlockValue;
		if (!ElectricToolDItem.IsOutOfPower(activeBlockValue))
		{
			CellFace cellFace = raycastResult.CellFace;
			int cellValue = miner.m_subsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z);
			_ = BlocksManager.Blocks[Terrain.ExtractContents(cellValue)];
			float num = miner.CalculateDigTime(BasaltBlock.Index, IronPickaxeBlock.Index);
			float num2 = miner.CalculateDigTime(cellValue, activeBlockValue);
			if (!(DigProgress >= 1f) || !(num2 < num))
			{
				return;
			}
			miner.DigCellFace = null;
			miner.Poke(forceRestart: true);
			Point3[] array = PointsGetter(cellFace);
			int num3 = 0;
			Point3[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				Point3 point = array2[i];
				int cellValue2 = miner.m_subsystemTerrain.Terrain.GetCellValue(raycastResult.CellFace.X + point.X, raycastResult.CellFace.Y + point.Y, raycastResult.CellFace.Z + point.Z);
				if (miner.CalculateDigTime(cellValue2, activeBlockValue) > num)
				{
					continue;
				}
				Block block = BlocksManager.Blocks[Terrain.ExtractContents(cellValue2)];
				if ((!(block is AirBlock) && !(block is FluidBlock)) || 1 == 0)
				{
					BlockPlacementData digValue = BlocksManager.Blocks[Terrain.ExtractContents(cellValue2)].GetDigValue(miner.m_subsystemTerrain, miner, cellValue2, activeBlockValue, raycastResult);
					miner.m_subsystemTerrain.DestroyCell(BlocksManager.Blocks[SteelPickaxeBlock.Index].GetToolLevel(SteelPickaxeBlock.Index), digValue.CellFace.X + point.X, digValue.CellFace.Y + point.Y, digValue.CellFace.Z + point.Z, digValue.Value, noDrop: true, noParticleSystem: false);
					Vector3 position = new Vector3(point) + new Vector3(digValue.CellFace.Point) + new Vector3(0.5f);
					miner.m_subsystemTerrain.m_subsystemPickables.AddPickable(cellValue2, 1, position, null, null);
					if (point == Point3.Zero)
					{
						miner.m_subsystemSoundMaterials.PlayImpactSound(cellValue, new Vector3(cellFace.X, cellFace.Y, cellFace.Z), 2f);
					}
					num3++;
				}
			}
			Digged = false;
			DamageableItemManager.DamageActiveTool(miner, num3);
		}
		else
		{
			DigProgress = 0f;
		}
	}
}
