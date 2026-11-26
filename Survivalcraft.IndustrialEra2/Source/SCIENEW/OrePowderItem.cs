using System.Collections.Generic;
using Engine;
using Game;
using RecipaediaEX;
using SCIENEW.Extensions.GameLogic;
using SCIENEW.RecipaediaEX.Loaders;
using SCIENEW.RecipaediaEX.Recipes;
using SCIENEW.Utils;

namespace SCIENEW;

public class OrePowderItem : FlatItem, IMaterialBlock, IProceduralRecipeProvider
{
	public static readonly Color[] Colors = new Color[22]
	{
		Color.Black,
		Color.White,
		new Color(255, 215, 0),
		new Color(212, 212, 212),
		new Color(87, 86, 85),
		new Color(232, 232, 232),
		new Color(64, 224, 205),
		new Color(225, 225, 225),
		new Color(60, 60, 60),
		Color.White,
		new Color(120, 120, 120),
		new Color(199, 97, 20),
		Color.LightGreen,
		Color.White,
		new Color(139, 69, 19),
		new Color(34, 139, 34),
		Color.Orange,
		new Color(205, 190, 112),
		Color.White,
		Color.White,
		Color.White,
		Color.DarkYellow
	};

	public Materials Material = Materials.Iron;

	private bool createRecipe;

	public Materials GetMaterial()
	{
		return Material;
	}

	public float GetMoltedVolume()
	{
		return 0f;
	}

	public OrePowderItem(Materials type, bool createRecipe = true)
		: this(type.ToString() + "OrePowder", Colors[(int)type], createRecipe)
	{
		Material = type;
		DisplayName = string.Format(LanguageControl.Get("Items", "Names", "OrePowderItem"), Material.ToDisplayName());
		Description = string.Format(LanguageControl.Get("Items", "Descriptions", "OrePowderItem"), Material.ToDisplayName());
	}

	public OrePowderItem(string id, Color color, bool createRecipe = true)
		: base(id)
	{
		Material = Materials.None;
		Id = id;
		Color = color;
		DefaultTextureSlot = 198;
		this.createRecipe = createRecipe;
	}

	public IEnumerable<IRecipe> GetProceduralRecipes()
	{
		if (createRecipe && Material != Materials.None)
		{
			yield return new One2OneRecipe
			{
				Description = string.Format(LanguageControl.Get("IECraftingRecipes", "Descriptions", "SCIE_OrePowderItem_0"), Material.ToDisplayName()),
				Ingredient = RecipesUtils.BlockToCratingID(ItemsBlockManager.GetItemIdByMaterial<OreChunkItem>(Material)),
				IngredientCount = 1,
				ResultID = ItemsBlockManager.GetItemID(this),
				ResultCount = 2,
				ProcessType = "Kibbling"
			};
		}
	}
}
