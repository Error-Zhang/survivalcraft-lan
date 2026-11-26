using System.Collections.Generic;
using Engine;
using Game;
using RecipaediaEX;
using SCIENEW.Extensions.GameLogic;
using SCIENEW.RecipaediaEX.Loaders;
using SCIENEW.RecipaediaEX.Recipes;
using SCIENEW.Utils;

namespace SCIENEW;

public class PowderItem : FlatItem, IMaterialBlock, IProceduralRecipeProvider
{
	public Materials Material;

	public Materials GetMaterial()
	{
		return Material;
	}

	public float GetMoltedVolume()
	{
		return 2f;
	}

	public PowderItem(Materials type)
		: this(type.ToString() + "Powder", type.GetColor())
	{
		Material = type;
		DisplayName = string.Format(LanguageControl.Get("Items", "Names", "PowderItem"), Material.ToDisplayName());
		Description = string.Format(LanguageControl.Get("Items", "Descriptions", "PowderItem"), Material.ToDisplayName());
	}

	public PowderItem(string id, Color color)
		: base(id)
	{
		Material = Materials.None;
		Color = color;
		DefaultTextureSlot = 198;
	}

	public override string GetCraftingId()
	{
		return Id;
	}

	public IEnumerable<IRecipe> GetProceduralRecipes()
	{
		if (Material != Materials.None)
		{
			yield return new One2OneRecipe
			{
				Description = string.Format(LanguageControl.Get("IECraftingRecipes", "Descriptions", "SCIE_PowderItem_0"), Material.ToDisplayName()),
				Ingredient = RecipesUtils.BlockToCratingID(ItemsBlockManager.GetItemIdByMaterial<MetalIngotItem>(Material)),
				IngredientCount = 1,
				ResultID = ItemsBlockManager.GetItemID(this),
				ResultCount = 1,
				ProcessType = "Kibbling"
			};
		}
	}
}
