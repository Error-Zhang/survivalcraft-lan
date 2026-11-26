using System;
using System.Collections.Generic;
using Engine;
using Game;
using RecipaediaEX;
using SCIENEW.Extensions.GameLogic;
using SCIENEW.RecipaediaEX.Loaders;
using SCIENEW.RecipaediaEX.Recipes;
using SCIENEW.Utils;

namespace SCIENEW;

public class MetalNBlock : DefaultNBlock, IMaterialBlock, IProceduralRecipeProvider
{
	public Materials Material;

	public Materials GetMaterial()
	{
		return Material;
	}

	public float GetMoltedVolume()
	{
		return 18f;
	}

	public MetalNBlock(Materials material)
		: base(material.ToString() + "NormalBlock", IndustrialModLoader.BaseTexture, (material == Materials.Steel) ? Color.White : material.GetColor(), (material == Materials.Steel) ? 228 : 180)
	{
		Material = material;
		DisplayName = string.Format(LanguageControl.Get("NormalBlocks", "Names", "MetalNBlock"), material.ToDisplayName());
		Description = string.Format(LanguageControl.Get("NormalBlocks", "Descriptions", "MetalNBlock"), material.ToDisplayName());
		Texture = ((material == Materials.Steel) ? IndustrialModLoader.IETexture : IndustrialModLoader.BaseTexture);
	}

	public override int GetMaxStacking(int value)
	{
		return 4;
	}

	public override float GetExplosionResilience(int value)
	{
		return Material.GetExplosionResilience();
	}

	public override float GetDigResilience(int value)
	{
		return Material.GetDigResilience();
	}

	public IEnumerable<IRecipe> GetProceduralRecipes()
	{
		IECraftingRecipe iECraftingRecipe = new IECraftingRecipe();
		iECraftingRecipe.ResultValue = NormalBlockManager.GetNormalBlockID(this);
		iECraftingRecipe.ResultCount = 1;
		iECraftingRecipe.RequiredHeatLevel = 0f;
		iECraftingRecipe.RequiredPlayerLevel = 1f;
		iECraftingRecipe.Ingredients = new string[36];
		iECraftingRecipe.Description = string.Format(LanguageControl.Get("IECraftingRecipes", "Descriptions", "SCIE_MetalNBlock_0"), Material.ToDisplayName(), Material.ToDisplayName());
		IECraftingRecipe iECraftingRecipe2 = iECraftingRecipe;
		iECraftingRecipe2.Ingredients[0] = (iECraftingRecipe2.Ingredients[1] = (iECraftingRecipe2.Ingredients[2] = (iECraftingRecipe2.Ingredients[6] = (iECraftingRecipe2.Ingredients[7] = (iECraftingRecipe2.Ingredients[8] = (iECraftingRecipe2.Ingredients[12] = (iECraftingRecipe2.Ingredients[13] = (iECraftingRecipe2.Ingredients[14] = RecipesUtils.BlockToCratingID(ItemsBlockManager.GetItemIdByMaterial<MetalIngotItem>(Material))))))))));
		iECraftingRecipe2.PreTransformIngredients();
		yield return iECraftingRecipe2;
		if (Material != Materials.None)
		{
			string processType = (((CastingUtils.IsHeatMetal(Material) ? ComponentHearthFurnaceCastingBasin.RequiredHeatLevel : ComponentBlastFurnaceCastingBasin.RequiredHeatLevel) == ComponentHearthFurnaceCastingBasin.RequiredHeatLevel) ? "HearthCasting" : "BlastCasting");
			yield return new CastingEquationRecipe
			{
				Description = string.Format(LanguageControl.Get("IECraftingRecipes", "Descriptions", "SCIE_MetalNBlock_1"), Material.ToDisplayName()),
				IngredientFluidValue = FluidsManager.GetFluidIDByMaterial<MoltenMetalFluid>(Material),
				IngredientFluidVolume = GetMoltedVolume(),
				ModuleValues = Array.Empty<int>(),
				ModuleCount = 0,
				ResultValue = NormalBlockManager.GetNormalBlockID(this),
				ResultCount = 1,
				ProcessType = processType,
				RequiredPlayerLevel = 0f
			};
		}
	}
}
