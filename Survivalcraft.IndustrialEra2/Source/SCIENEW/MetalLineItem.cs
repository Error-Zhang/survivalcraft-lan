using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using RecipaediaEX;
using SCIENEW.Extensions.GameLogic;
using SCIENEW.RecipaediaEX.Loaders;
using SCIENEW.RecipaediaEX.Recipes;
using SCIENEW.Utils;

namespace SCIENEW;

public class MetalLineItem : FlatItem, IMaterialBlock, IProceduralRecipeProvider
{
	public Materials Material;

	public Materials GetMaterial()
	{
		return Material;
	}

	public float GetMoltedVolume()
	{
		return 0.25f;
	}

	public MetalLineItem(Materials type)
		: base(type.ToString() + "Line")
	{
		Material = type;
		DisplayName = string.Format(LanguageControl.Get("Items", "Names", "MetalLineItem"), Material.ToDisplayName());
		DefaultTextureSlot = 212;
		Description = string.Format(LanguageControl.Get("Items", "Descriptions", "MetalLineItem"), Material.ToDisplayName());
		Color = type.GetColor();
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawFlatBlock(primitivesRenderer, value, size, ref matrix, IndustrialModLoader.IETexture, Color, isEmissive: false, environmentData);
	}

	public IEnumerable<IRecipe> GetProceduralRecipes()
	{
		if (Material != Materials.None)
		{
			yield return new One2OneRecipe
			{
				Description = string.Format(LanguageControl.Get("IECraftingRecipes", "Descriptions", "SCIE_MetalLineItem_0"), Material.ToDisplayName()),
				Ingredient = RecipesUtils.BlockToCratingID(ItemsBlockManager.GetItemIdByMaterial<RodItem>(Material)),
				IngredientCount = 1,
				ResultID = ItemsBlockManager.GetItemID(this),
				ResultCount = 4,
				ProcessType = "Squeezing"
			};
		}
	}
}
