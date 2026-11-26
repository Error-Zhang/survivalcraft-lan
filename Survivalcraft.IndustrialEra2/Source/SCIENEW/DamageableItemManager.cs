using System;
using System.Collections.Generic;
using System.Linq;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public static class DamageableItemManager
{
	public static DamageableItemBlock[] Items;

	public static Dictionary<string, int> IdMap;

	public static void Init()
	{
		Items = new DamageableItemBlock[36]
		{
			new DrillDItem("SteelDrill", Color.White, 1000),
			new DrillDItem("DiamondDrill", Color.Cyan, 2000),
			new TubularisDItem("IronTubularis", Color.White, 700),
			new TubularisDItem("SteelTubularis", Color.Gray, 1100),
			new BatteryDItem("PbSO4Battery", 1000, "Models/PbSO4Battery", "PbSO4Battery", IndustrialModLoader.IETexture, Color.White, Matrix.Identity, Matrix.Identity, 1.5f),
			new BatteryDItem("AlNaOHBattery", 1500, "Models/PbSO4Battery", "PbSO4Battery", IndustrialModLoader.IETexture, Color.Gray, Matrix.Identity, Matrix.Identity, 1.5f),
			new ElectricToolDItem("Saw", 2000, new Type[2]
			{
				typeof(SawDItemBehavior),
				typeof(ChargeByBatteryDItemBehavior)
			}, "Models/Saw", "Saw", ContentManager.Get<Texture2D>("Textures/Saw"), Color.White, Matrix.Identity, Matrix.Identity, new Vector3(-0.85f, 0.35f, 0f), new Vector3(-1f, 0.5f, -1f), 0.7f, 1f, 0f, 18f),
			new MiningDrillDItem("Crossing", (CellFace face) =>
			{
				DevicesUtils.FaceToAxesAndConner(face.Face, out Point3[] axes, out Point3[] _);
				return axes.Append(Point3.Zero).Append(CellFace.FaceToPoint3(CellFace.OppositeFace(face.Face))).ToArray();
			}, display: true),
			new ElectricToolDItem("Drill", 1000, new Type[2]
			{
				typeof(DrillDItemBehavior),
				typeof(ChargeByBatteryDItemBehavior)
			}, "Models/Drill", "Drill", ContentManager.Get<Texture2D>("Textures/Drill"), Color.White, Matrix.Identity, Matrix.Identity, Vector3.Zero, new Vector3(-1f, 0.5f, -1f), 1f),
			new HookDItem(),
			new CoatingBucketDItem(0),
			new CoatingBucketDItem(1),
			new CoatingBucketDItem(2),
			new CoatingBucketDItem(3),
			new CoatingBucketDItem(4),
			new CoatingBucketDItem(5),
			new CoatingBucketDItem(6),
			new CoatingBucketDItem(7),
			new CoatingBucketDItem(8),
			new CoatingBucketDItem(9),
			new CoatingBucketDItem(10),
			new CoatingBucketDItem(11),
			new CoatingBucketDItem(12),
			new CoatingBucketDItem(13),
			new CoatingBucketDItem(14),
			new CoatingBucketDItem(15),
			new CoatingStripperBucketDItem(),
			new PlasticRotorDItem(),
			new WoodRotorDItem(),
			new MiningDrillDItem("Single", (CellFace _) => new Point3[1] { Point3.Zero }, display: false),
			new MiningDrillDItem("Triple", (CellFace face) => new Point3[3]
			{
				Point3.Zero,
				CellFace.FaceToPoint3(CellFace.OppositeFace(face.Face)),
				CellFace.FaceToPoint3(CellFace.OppositeFace(face.Face)) * 2
			}, display: false),
			new MiningDrillDItem("3x3", (CellFace face) =>
			{
				DevicesUtils.FaceToAxesAndConner(face.Face, out Point3[] axes, out Point3[] conners);
				return axes.Concat(conners).Append(Point3.Zero).ToArray();
			}, display: false),
			new FuelRodDItem("UFuelRod", 2000, FuelRodDItem.RodType.UFuelRod, Color.LightGreen),
			new FuelRodDItem("PFuelRod", 2000, FuelRodDItem.RodType.PFuelRod, Color.DarkGray),
			new FuelRodDItem("ControlRod", 2000, FuelRodDItem.RodType.ControlRod, Color.LightYellow),
			new FuelRodDItem("CarbonRod", 2000, FuelRodDItem.RodType.CarbonRod, Color.Black)
		};
		IdMap = new Dictionary<string, int>();
		int num = 0;
		DamageableItemBlock[] items = Items;
		foreach (DamageableItemBlock damageableItemBlock in items)
		{
			int data = num & 0x3F;
			int value = Terrain.MakeBlockValue(BaseDamageableItemBlock.Index, 0, data);
			IdMap.Add(damageableItemBlock.GetCraftingId(), value);
			num++;
		}
	}

	public static DamageableItemBlock GetItem(int value)
	{
		int num = Terrain.ExtractData(value) & 0x3F;
		if (num < Items.Length)
		{
			return Items[num];
		}
		return Items[0];
	}

	public static DamageableItemBlock GetItem(string craftingID)
	{
		return GetItem(IdMap[craftingID]);
	}

	public static int GetItemID(string id)
	{
		return IdMap[id];
	}

	public static string GetItemCraftingID(int value)
	{
		return GetItem(value).GetCraftingId();
	}

	public static int GetDamage(int value)
	{
		return Terrain.ExtractData(value) >> 6;
	}

	public static int SetDamage(int value, int damage)
	{
		return Terrain.ReplaceData(value, (Terrain.ExtractData(value) & 0x3F) | (damage << 6));
	}

	public static void DamageActiveTool(ComponentMiner componentMiner, int damageCount)
	{
		if (componentMiner.m_subsystemGameInfo.WorldSettings.GameMode == GameMode.Creative || componentMiner.Inventory == null)
		{
			return;
		}
		int num = DamageItem(componentMiner.ActiveBlockValue, damageCount, componentMiner.Entity);
		if (num != 0)
		{
			int slotCount = componentMiner.Inventory.GetSlotCount(componentMiner.Inventory.ActiveSlotIndex);
			componentMiner.Inventory.RemoveSlotItems(componentMiner.Inventory.ActiveSlotIndex, slotCount);
			if (componentMiner.Inventory.GetSlotCount(componentMiner.Inventory.ActiveSlotIndex) == 0)
			{
				componentMiner.Inventory.AddSlotItems(componentMiner.Inventory.ActiveSlotIndex, num, slotCount);
			}
		}
		else
		{
			componentMiner.Inventory.RemoveSlotItems(componentMiner.Inventory.ActiveSlotIndex, 1);
		}
	}

	public static int DamageItem(int value, int damageCount, Entity owner = null)
	{
		Block block = BlocksManager.Blocks[Terrain.ExtractContents(value)];
		int result = 0;
		bool skipVanilla = false;
		ModsManager.HookAction("DamageItem", (ModLoader modLoader) =>
		{
			result = modLoader.DamageItem(block, value, damageCount, owner, out skipVanilla);
			return false;
		});
		if (skipVanilla)
		{
			return result;
		}
		int durability = block.GetDurability(value);
		if (durability < 0)
		{
			return value;
		}
		int num = block.GetDamage(value) + damageCount;
		if (num <= 0 || num > durability)
		{
			return block.GetDamageDestructionValue(value);
		}
		return block.SetDamage(value, num);
	}

	public static T GetItem<T>(int index = 0) where T : class
	{
		return Items.Where((DamageableItemBlock x) => x.GetType() == typeof(T)).ElementAt(index) as T;
	}

	public static FilteredDItemsEnumerable<T> GetItems<T>() where T : DamageableItemBlock
	{
		return new FilteredDItemsEnumerable<T>();
	}
}
