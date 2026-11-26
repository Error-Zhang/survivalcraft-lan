using System;
using System.Collections.Generic;
using Engine;
using Game;
using RecipaediaEX;
using SCIENEW.Extensions.GameLogic;
using SCIENEW.RecipaediaEX.Loaders;
using SCIENEW.RecipaediaEX.Recipes;

namespace SCIENEW;

public class MoltenMetalFluid : Fluid, IMaterialBlock, ITroughTransportableFluid, IProceduralRecipeProvider
{
	public Materials Material;

	public float GetMoltedVolume()
	{
		return 0f;
	}

	public MoltenMetalFluid(Materials material)
	{
		Material = material;
	}

	public override string GetDisplayName(int value)
	{
		return string.Format(LanguageControl.Get("Fluids", "Names", "MoltenMetalFluid"), Material.ToDisplayName());
	}

	public override string GetCraftingID(int value)
	{
		return "Molten" + Material;
	}

	public override string GetDescription(int value)
	{
		return string.Format(LanguageControl.Get("Fluids", "Descriptions", "MoltenMetalFluid"), Material.ToDisplayName());
	}

	public override int GetCongenericFluidCount()
	{
		return 1;
	}

	public override Color GetColor(int value)
	{
		return Material.GetMetalColor();
	}

	public Materials GetMaterial()
	{
		return Material;
	}

	public IEnumerable<IRecipe> GetProceduralRecipes()
	{
		foreach (Type materialBlockType in MaterialBlocksController.m_materialBlockTypes)
		{
			int num = 0;
			IMaterialBlock materialBlock = null;
			if (materialBlockType.IsAssignableTo(typeof(ItemBlock)))
			{
				num = ItemsBlockManager.GetItemIdByMaterial(materialBlockType, Material);
				materialBlock = ItemsBlockManager.GetItemByMaterial(materialBlockType, Material) as IMaterialBlock;
			}
			else if (materialBlockType.IsAssignableTo(typeof(NormalBlock)))
			{
				num = NormalBlockManager.GetNBlockIdByMaterial(materialBlockType, Material);
				materialBlock = NormalBlockManager.GetNBlockByMaterial(materialBlockType, Material) as IMaterialBlock;
			}
			if (num != 0 && materialBlock != null && materialBlock.GetMoltedVolume() != 0f)
			{
				yield return new MoltingEquationRecipe
				{
					Description = string.Format(LanguageControl.Get("Equations", "Descriptions", "InductionFurnace.MoltenMetalFluid.FromAnyMaterial"), Material.ToDisplayName()),
					IngredientValue = num,
					IngredientCount = 1,
					ResultFluidValue = FluidsManager.GetFluidID(this),
					ResultFluidVolume = materialBlock.GetMoltedVolume(),
					RequiredPlayerLevel = 0f
				};
			}
		}
	}
}
