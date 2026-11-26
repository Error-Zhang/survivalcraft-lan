using Game;
using SCIENEW.Utils;

namespace SCIENEW;

public static class CastingUtils
{
	public static bool IsHeatMetal(Materials material)
	{
		return material switch
		{
			Materials.Iron => false,
			Materials.Steel => true,
			_ => MaterialsUtils.materialMeltingPoints[material] > 1500f,
		};
	}

	public static float GetIronItemVolume(int value)
	{
		int num = Terrain.ExtractContents(value);
		if (num == IronIngotBlock.Index)
		{
			return 2f;
		}
		if (num == RodBlock.Index)
		{
			return 1f;
		}
		if (num == IronBlock.Index)
		{
			return 18f;
		}
		if (num == BaseItemBlock.Index)
		{
			ItemBlock item = ItemsBlockManager.GetItem(value);
			if (item is IMaterialBlock materialBlock && materialBlock.GetMaterial() == Materials.Iron)
			{
				if (item is PlateItem { isSmall: false })
				{
					return 2f;
				}
				if (item is PlateItem { isSmall: not false })
				{
					return 1f / 18f;
				}
				if (item is RodItem)
				{
					return 1f;
				}
				if (item is MetalLineItem)
				{
					return 0.25f;
				}
				if (item is PowderItem)
				{
					return 2f;
				}
				if (item is OrePowderItem)
				{
					return 2f;
				}
				if (item is ScrapIronItem)
				{
					return 2f;
				}
				if (item is SteelGearItem)
				{
					return 2f;
				}
				if (item is SteelWheelItem)
				{
					return 2f;
				}
				if (item is UIronSheetItem)
				{
					return 2f;
				}
			}
		}
		else if (num == BaseNormalBlock.Index)
		{
			NormalBlock normalBlock = NormalBlockManager.GetNormalBlock(value);
			if (normalBlock is IMaterialBlock materialBlock2 && materialBlock2.GetMaterial() == Materials.Iron && normalBlock is MetalNBlock)
			{
				return 18f;
			}
		}
		return -1f;
	}

	public static float GetSteelItemVolume(int value)
	{
		int num = Terrain.ExtractContents(value);
		if (num == BaseItemBlock.Index)
		{
			ItemBlock item = ItemsBlockManager.GetItem(value);
			if (item is IMaterialBlock materialBlock && materialBlock.GetMaterial() == Materials.Steel)
			{
				if (item is PlateItem { isSmall: false })
				{
					return 2f;
				}
				if (item is PlateItem { isSmall: not false })
				{
					return 1f / 18f;
				}
				if (item is RodItem)
				{
					return 1f;
				}
				if (item is MetalLineItem)
				{
					return 0.25f;
				}
				if (item is PowderItem)
				{
					return 2f;
				}
				if (item is OrePowderItem)
				{
					return 2f;
				}
				if (item is SteelGearItem)
				{
					return 2f;
				}
				if (item is SteelWheelItem)
				{
					return 2f;
				}
				if (item is MetalIngotItem)
				{
					return 2f;
				}
			}
		}
		else if (num == BaseNormalBlock.Index)
		{
			NormalBlock normalBlock = NormalBlockManager.GetNormalBlock(value);
			if (normalBlock is IMaterialBlock materialBlock2 && materialBlock2.GetMaterial() == Materials.Steel && normalBlock is MetalNBlock)
			{
				return 18f;
			}
		}
		return -1f;
	}
}
