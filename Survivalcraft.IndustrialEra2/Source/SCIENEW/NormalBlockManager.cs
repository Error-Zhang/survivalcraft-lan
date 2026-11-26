using System;
using System.Collections.Generic;
using System.Linq;
using Engine;
using Game;

namespace SCIENEW;

public static class NormalBlockManager
{
	public static NormalBlock[] Blocks;

	public static Dictionary<string, int> IdMap;

	public static void Init()
	{
		Blocks = new NormalBlock[77]
		{
			new MachineShellNBlock("BasicMechanicalShell", 107),
			new FlaxNBlock("Flax_Top", Color.White, 7, new int[1] { ItemsBlockManager.GetItemID("FlaxSeed") }),
			new FlaxNBlock("Flax_Stem_Ripened", Color.White, 9, new int[3]
			{
				ItemsBlockManager.GetItemID("FlaxSeed"),
				ItemsBlockManager.GetItemID("FlaxFiber"),
				ItemsBlockManager.GetItemID("FlaxFiber")
			}),
			new FlaxNBlock("Flax_Stem", Color.White, 10, new int[1] { ItemsBlockManager.GetItemID("FlaxFiber") }),
			new MetalNBlock(Materials.Steel),
			new MetalNBlock(Materials.Gold),
			new MetalNBlock(Materials.Silver),
			new MetalNBlock(Materials.Lead),
			new MetalNBlock(Materials.Platinum),
			new MetalNBlock(Materials.Zinc),
			new MetalNBlock(Materials.Stannary),
			new MetalNBlock(Materials.Chromium),
			new MetalNBlock(Materials.Titanium),
			new MetalNBlock(Materials.Nickel),
			new MetalNBlock(Materials.Aluminum),
			new MetalNBlock(Materials.Germanium),
			new MetalNBlock(Materials.FeAlCrAlloy),
			new MetalNBlock(Materials.Brass),
			new MetalNBlock(Materials.Vanadium),
			new MetalNBlock(Materials.Cupronickel),
			new StoneTypeNBlock("FirebrickWall", IndustrialModLoader.IETexture, Color.White, 4),
			new ConcreteWallNBlock("ConcreteWall", IndustrialModLoader.IETexture, Color.White, 50, 4000),
			new CokeCoalNBlock(),
			new MachineShellNBlock("AdvancedMechanicalShell", 99),
			new OilSludgeNBlock(),
			new ConcreteWallNBlock("HighStrengthConcreteWall", IndustrialModLoader.IETexture, Color.White, 20, 4500, 30),
			new ConcreteWallNBlock("PrestressedConcreteWall", IndustrialModLoader.IETexture, Color.White, 49, 4800, 32),
			new ConcreteWallNBlock("ConcreteWall", IndustrialModLoader.IETexture, Color.White, 53, 4000, 24, 0),
			new ConcreteWallNBlock("ConcreteWall", IndustrialModLoader.IETexture, Color.White, 53, 4000, 24, 1),
			new ConcreteWallNBlock("ConcreteWall", IndustrialModLoader.IETexture, Color.White, 53, 4000, 24, 2),
			new ConcreteWallNBlock("ConcreteWall", IndustrialModLoader.IETexture, Color.White, 53, 4000, 24, 3),
			new ConcreteWallNBlock("ConcreteWall", IndustrialModLoader.IETexture, Color.White, 53, 4000, 24, 4),
			new ConcreteWallNBlock("ConcreteWall", IndustrialModLoader.IETexture, Color.White, 53, 4000, 24, 5),
			new ConcreteWallNBlock("ConcreteWall", IndustrialModLoader.IETexture, Color.White, 53, 4000, 24, 6),
			new ConcreteWallNBlock("ConcreteWall", IndustrialModLoader.IETexture, Color.White, 53, 4000, 24, 7),
			new ConcreteWallNBlock("ConcreteWall", IndustrialModLoader.IETexture, Color.White, 53, 4000, 24, 8),
			new ConcreteWallNBlock("ConcreteWall", IndustrialModLoader.IETexture, Color.White, 53, 4000, 24, 9),
			new ConcreteWallNBlock("ConcreteWall", IndustrialModLoader.IETexture, Color.White, 53, 4000, 24, 10),
			new ConcreteWallNBlock("ConcreteWall", IndustrialModLoader.IETexture, Color.White, 53, 4000, 24, 11),
			new ConcreteWallNBlock("ConcreteWall", IndustrialModLoader.IETexture, Color.White, 53, 4000, 24, 12),
			new ConcreteWallNBlock("ConcreteWall", IndustrialModLoader.IETexture, Color.White, 53, 4000, 24, 13),
			new ConcreteWallNBlock("ConcreteWall", IndustrialModLoader.IETexture, Color.White, 53, 4000, 24, 14),
			new ConcreteWallNBlock("ConcreteWall", IndustrialModLoader.IETexture, Color.White, 53, 4000, 24, 15),
			new ConcreteWallNBlock("HighStrengthConcreteWall", IndustrialModLoader.IETexture, Color.White, 51, 4500, 30, 0),
			new ConcreteWallNBlock("HighStrengthConcreteWall", IndustrialModLoader.IETexture, Color.White, 51, 4500, 30, 1),
			new ConcreteWallNBlock("HighStrengthConcreteWall", IndustrialModLoader.IETexture, Color.White, 51, 4500, 30, 2),
			new ConcreteWallNBlock("HighStrengthConcreteWall", IndustrialModLoader.IETexture, Color.White, 51, 4500, 30, 3),
			new ConcreteWallNBlock("HighStrengthConcreteWall", IndustrialModLoader.IETexture, Color.White, 51, 4500, 30, 4),
			new ConcreteWallNBlock("HighStrengthConcreteWall", IndustrialModLoader.IETexture, Color.White, 51, 4500, 30, 5),
			new ConcreteWallNBlock("HighStrengthConcreteWall", IndustrialModLoader.IETexture, Color.White, 51, 4500, 30, 6),
			new ConcreteWallNBlock("HighStrengthConcreteWall", IndustrialModLoader.IETexture, Color.White, 51, 4500, 30, 7),
			new ConcreteWallNBlock("HighStrengthConcreteWall", IndustrialModLoader.IETexture, Color.White, 51, 4500, 30, 8),
			new ConcreteWallNBlock("HighStrengthConcreteWall", IndustrialModLoader.IETexture, Color.White, 51, 4500, 30, 9),
			new ConcreteWallNBlock("HighStrengthConcreteWall", IndustrialModLoader.IETexture, Color.White, 51, 4500, 30, 10),
			new ConcreteWallNBlock("HighStrengthConcreteWall", IndustrialModLoader.IETexture, Color.White, 51, 4500, 30, 11),
			new ConcreteWallNBlock("HighStrengthConcreteWall", IndustrialModLoader.IETexture, Color.White, 51, 4500, 30, 12),
			new ConcreteWallNBlock("HighStrengthConcreteWall", IndustrialModLoader.IETexture, Color.White, 51, 4500, 30, 13),
			new ConcreteWallNBlock("HighStrengthConcreteWall", IndustrialModLoader.IETexture, Color.White, 51, 4500, 30, 14),
			new ConcreteWallNBlock("HighStrengthConcreteWall", IndustrialModLoader.IETexture, Color.White, 51, 4500, 30, 15),
			new ConcreteWallNBlock("PrestressedConcreteWall", IndustrialModLoader.IETexture, Color.White, 52, 4800, 32, 0),
			new ConcreteWallNBlock("PrestressedConcreteWall", IndustrialModLoader.IETexture, Color.White, 52, 4800, 32, 1),
			new ConcreteWallNBlock("PrestressedConcreteWall", IndustrialModLoader.IETexture, Color.White, 52, 4800, 32, 2),
			new ConcreteWallNBlock("PrestressedConcreteWall", IndustrialModLoader.IETexture, Color.White, 52, 4800, 32, 3),
			new ConcreteWallNBlock("PrestressedConcreteWall", IndustrialModLoader.IETexture, Color.White, 52, 4800, 32, 4),
			new ConcreteWallNBlock("PrestressedConcreteWall", IndustrialModLoader.IETexture, Color.White, 52, 4800, 32, 5),
			new ConcreteWallNBlock("PrestressedConcreteWall", IndustrialModLoader.IETexture, Color.White, 52, 4800, 32, 6),
			new ConcreteWallNBlock("PrestressedConcreteWall", IndustrialModLoader.IETexture, Color.White, 52, 4800, 32, 7),
			new ConcreteWallNBlock("PrestressedConcreteWall", IndustrialModLoader.IETexture, Color.White, 52, 4800, 32, 8),
			new ConcreteWallNBlock("PrestressedConcreteWall", IndustrialModLoader.IETexture, Color.White, 52, 4800, 32, 9),
			new ConcreteWallNBlock("PrestressedConcreteWall", IndustrialModLoader.IETexture, Color.White, 52, 4800, 32, 10),
			new ConcreteWallNBlock("PrestressedConcreteWall", IndustrialModLoader.IETexture, Color.White, 52, 4800, 32, 11),
			new ConcreteWallNBlock("PrestressedConcreteWall", IndustrialModLoader.IETexture, Color.White, 52, 4800, 32, 12),
			new ConcreteWallNBlock("PrestressedConcreteWall", IndustrialModLoader.IETexture, Color.White, 52, 4800, 32, 13),
			new ConcreteWallNBlock("PrestressedConcreteWall", IndustrialModLoader.IETexture, Color.White, 52, 4800, 32, 14),
			new ConcreteWallNBlock("PrestressedConcreteWall", IndustrialModLoader.IETexture, Color.White, 52, 4800, 32, 15),
			new MagnetiteNBlock(),
			new PolymetallicNoduleNBlock()
		};
		IdMap = new Dictionary<string, int>();
		int num = 0;
		NormalBlock[] blocks = Blocks;
		foreach (NormalBlock normalBlock in blocks)
		{
			int value = Terrain.MakeBlockValue(BaseNormalBlock.Index, 0, num++);
			IdMap.Add(normalBlock.GetCraftingId(), value);
		}
	}

	public static void LateInit()
	{
	}

	public static NormalBlock GetNormalBlock(int value)
	{
		int num = Terrain.ExtractData(value);
		if (num < Blocks.Length)
		{
			return Blocks[num];
		}
		return Blocks[0];
	}

	public static NormalBlock GetNormalBlock(string craftingID)
	{
		return GetNormalBlock(IdMap[craftingID]);
	}

	public static int GetNormalBlockID(string craftingID)
	{
		if (!IdMap.ContainsKey(craftingID))
		{
			return -1;
		}
		return IdMap[craftingID];
	}

	public static int GetNormalBlockID(NormalBlock block)
	{
		return GetNormalBlockID(block.GetCraftingId());
	}

	public static string GetNormalBlockCraftingID(int value)
	{
		return GetNormalBlock(value).GetCraftingId();
	}

	public static NormalBlock? GetNBlockByMaterial(Type itemType, Materials material)
	{
		if (!typeof(NormalBlock).IsAssignableFrom(itemType) || !typeof(IMaterialBlock).IsAssignableFrom(itemType))
		{
			throw new ArgumentException("itemType must be of type ItemBlock and implement IMaterialBlock");
		}
		NormalBlock[] array = Blocks.Where((NormalBlock x) => x.GetType() == itemType).ToArray();
		foreach (NormalBlock normalBlock in array)
		{
			if (!(normalBlock is IMaterialBlock materialBlock) || materialBlock.GetMaterial() == material)
			{
				return normalBlock;
			}
		}
		return null;
	}

	public static T? GetNBlockByMaterial<T>(Materials material) where T : NormalBlock, IMaterialBlock
	{
		NormalBlock[] array = Blocks.Where((NormalBlock x) => x.GetType() == typeof(T)).ToArray();
		foreach (NormalBlock normalBlock in array)
		{
			IMaterialBlock materialBlock = (normalBlock as IMaterialBlock) ?? null;
			if (materialBlock == null || materialBlock.GetMaterial() == material)
			{
				return normalBlock as T;
			}
		}
		return null;
	}

	public static int GetNBlockIdByMaterial(Type type, Materials material)
	{
		if (!typeof(NormalBlock).IsAssignableFrom(type) || !typeof(IMaterialBlock).IsAssignableFrom(type))
		{
			throw new ArgumentException("itemType must be of type ItemBlock and implement IMaterialBlock");
		}
		switch (material)
		{
		case Materials.Iron:
			if (type == typeof(MetalNBlock))
			{
				return IronBlock.Index;
			}
			break;
		case Materials.Copper:
			if (type == typeof(MetalNBlock))
			{
				return CopperBlock.Index;
			}
			break;
		case Materials.Germanium:
			if (type == typeof(MetalNBlock))
			{
				return SemiconductorBlock.Index;
			}
			break;
		}
		NormalBlock nBlockByMaterial = GetNBlockByMaterial(type, material);
		if (nBlockByMaterial != null)
		{
			return GetNormalBlockID(nBlockByMaterial);
		}
		return 0;
	}

	public static int GetNBlockIdByMaterial<T>(Materials material) where T : NormalBlock, IMaterialBlock
	{
		switch (material)
		{
		case Materials.Iron:
			if (typeof(T) == typeof(MetalNBlock))
			{
				return IronBlock.Index;
			}
			break;
		case Materials.Copper:
			if (typeof(T) == typeof(MetalNBlock))
			{
				return CopperBlock.Index;
			}
			break;
		case Materials.Germanium:
			if (typeof(T) == typeof(MetalNBlock))
			{
				return SemiconductorBlock.Index;
			}
			break;
		}
		T nBlockByMaterial = GetNBlockByMaterial<T>(material);
		if (nBlockByMaterial != null)
		{
			return GetNormalBlockID(nBlockByMaterial);
		}
		return 0;
	}

	public static T? GetNBlockByColor<T>(int? color) where T : NormalBlock, IPaintableBlock
	{
		NormalBlock[] array = Blocks.Where((NormalBlock x) => x.GetType() == typeof(T)).ToArray();
		foreach (NormalBlock normalBlock in array)
		{
			IPaintableBlock paintableBlock = (normalBlock as IPaintableBlock) ?? null;
			if (paintableBlock == null || paintableBlock.GetPaintColor() == color)
			{
				return normalBlock as T;
			}
		}
		return null;
	}

	public static int GetNBlockIdByColor<T>(int? color) where T : NormalBlock, IPaintableBlock
	{
		T nBlockByColor = GetNBlockByColor<T>(color);
		if (nBlockByColor != null)
		{
			return GetNormalBlockID(nBlockByColor);
		}
		return 0;
	}
}
