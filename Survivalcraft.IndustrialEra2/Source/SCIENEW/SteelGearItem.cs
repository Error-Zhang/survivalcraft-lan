using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using RecipaediaEX.Implementation;
using SCIENEW.Extensions.GameLogic;
using SCIENEW.RecipaediaEX;
using SCIENEW.Utils;

namespace SCIENEW;

public class SteelGearItem : MeshItem, IMaterialBlock
{
	public SteelGearItem()
		: base("SteelGear", "Models/Gear", "Gear", ContentManager.Get<Texture2D>("Textures/Iron"), Materials.Steel.GetColor(), Matrix.CreateRotationX(MathUtils.DegToRad(90f)) * Matrix.CreateTranslation(new Vector3(0f, 0.4f, 0f)) * 2f, Matrix.Identity)
	{
	}

	public Materials GetMaterial()
	{
		return Materials.Iron;
	}

	public float GetMoltedVolume()
	{
		return 2f;
	}

	public override IRecipe? GetDynamicRecipe(IRecipe actual, Project project)
	{
		if (!DynamicRecipeUtils.TryResolveCraftingContext(actual, project, out SubsystemTerrain _, out FormattedRecipe formattedRecipe, out string[] _, out float heatLevel, out float _))
		{
			return null;
		}
		string[] craftingIngredientStrings = DynamicRecipeUtils.GetCraftingIngredientStrings(formattedRecipe, 0f);
		if (craftingIngredientStrings == null)
		{
			return null;
		}
		string[] array = new string[36];
		array[1] = "I-" + ItemsBlockManager.GetItemByMaterial<MetalIngotItem>(Materials.Steel).GetCraftingId();
		array[6] = "I-" + ItemsBlockManager.GetItemByMaterial<MetalIngotItem>(Materials.Steel).GetCraftingId();
		array[8] = "I-" + ItemsBlockManager.GetItemByMaterial<MetalIngotItem>(Materials.Steel).GetCraftingId();
		array[13] = "I-" + ItemsBlockManager.GetItemByMaterial<MetalIngotItem>(Materials.Steel).GetCraftingId();
		Block hammerBlock = BlocksManager.Blocks[IronHammerBlock.Index];
		int num = craftingIngredientStrings.FirstIndex((string x) => x?.Contains(hammerBlock.GetCraftingId(IronHammerBlock.Index)) ?? false);
		if (num == -1)
		{
			return null;
		}
		string text = craftingIngredientStrings[num];
		RecipesUtils.DecodeIEIngredient(text, out string _, out int? data);
		if (!data.HasValue)
		{
			return null;
		}
		int value = Terrain.MakeBlockValue(IronHammerBlock.Index, 0, data.Value);
		craftingIngredientStrings[num] = null;
		if (!RecipesUtils.MatchRecipe(array, craftingIngredientStrings))
		{
			craftingIngredientStrings[num] = text;
			return null;
		}
		craftingIngredientStrings[num] = text;
		if (hammerBlock.GetDurability(value) - hammerBlock.GetDamage(value) < 10)
		{
			return null;
		}
		int remainsValue = BlocksManager.DamageItem(value, 10);
		return DynamicRecipeUtils.FromCraftingRecipe(new CraftingRecipe
		{
			ResultValue = ItemsBlockManager.GetItemID(this),
			ResultCount = 4,
			RemainsValue = remainsValue,
			RemainsCount = 1,
			Ingredients = craftingIngredientStrings,
			RequiredHeatLevel = heatLevel
		});
	}
}
